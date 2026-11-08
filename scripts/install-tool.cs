#!/usr/bin/env dotnet
#:property PublishAot=false
#nullable enable

// Build, install, and manage claude-team as a user-global .NET tool.
// The NuGet tool package (PackAsTool=true in src/ClaudeTeam/ClaudeTeam.csproj) is the way
// to build, install, and launch claude-team.
//
// Run the script from any directory. The script finds the checkout from its own file path:
//   dotnet run --file scripts/install-tool.cs                   # status + usage (default)
//   dotnet run --file scripts/install-tool.cs -- install        # pack + install/update the user-global tool
//   dotnet run --file scripts/install-tool.cs -- reinstall      # stop running instances, then install
//   dotnet run --file scripts/install-tool.cs -- uninstall      # remove the user-global tool
//   dotnet run --file scripts/install-tool.cs -- status         # explicit form of the default
//   dotnet run --file scripts/install-tool.cs -- --help         # usage only; writes nothing
//
// `status` and `--help` write nothing. `install`, `reinstall`, and `uninstall` change the
// user-global dotnet tool store, which is outside the workspace.
//
// `reinstall` exists because Windows holds an exclusive lock on the shim executable while the
// tool runs. The lock makes `install` fail on the uninstall/install step. Unix replaces the file
// of a running executable, so on Unix the stop step only ends a process that runs the old build.
//
// Self-location: the checkout root is the parent of the `scripts/` directory of this script.
// The script resolves it through [CallerFilePath], never from the working directory. The copy of
// this script in a worktree always packs and installs the code of that worktree.

using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

const string PackageId = "claude-team";
const string ToolCommandName = "claude-team";
const int StopTimeoutMilliseconds = 10_000;

var checkoutRoot = FindCheckoutRoot();
var gitDir = Path.Combine(checkoutRoot, ".git");
if (!Directory.Exists(gitDir) && !File.Exists(gitDir))
{
    Console.Error.WriteLine($"Not a git checkout: {checkoutRoot} has no .git directory or file.");
    return 1;
}

var command = args.Length > 0 ? args[0] : "status";
var commandArgs = args.Skip(1).ToArray();

return (command, commandArgs) switch
{
    ("--help" or "-h" or "help", []) => RunHelp(),
    ("status", []) => RunStatus(checkoutRoot),
    ("install", []) => RunInstall(checkoutRoot),
    ("reinstall", []) => RunReinstall(checkoutRoot),
    ("uninstall", []) => RunUninstall(),
    _ => UnknownCommand(command, commandArgs)
};

static int RunHelp()
{
    PrintUsage();
    return 0;
}

static int UnknownCommand(string command, string[] commandArgs)
{
    var full = string.Join(' ', new[] { command }.Concat(commandArgs));
    Console.Error.WriteLine($"Unknown command: {full}");
    PrintUsage();
    return 1;
}

static void PrintUsage()
{
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --file scripts/install-tool.cs                status (default)");
    Console.WriteLine("  dotnet run --file scripts/install-tool.cs -- install     install/update the user-global tool");
    Console.WriteLine("  dotnet run --file scripts/install-tool.cs -- reinstall   stop running instances, then install");
    Console.WriteLine("  dotnet run --file scripts/install-tool.cs -- uninstall   remove the user-global tool");
    Console.WriteLine("  dotnet run --file scripts/install-tool.cs -- status      explicit form of the default");
    Console.WriteLine("  dotnet run --file scripts/install-tool.cs -- --help      usage only");
}

static int RunStatus(string checkoutRoot)
{
    var globalVersion = GetInstalledVersion();

    Console.WriteLine(globalVersion is null
        ? "Global tool: not installed."
        : $"Global tool: {PackageId} {globalVersion}");

    var branch = GetCurrentBranch(checkoutRoot);
    Console.WriteLine($"This checkout: branch {branch ?? "(detached HEAD)"}.");

    if (IsDirty(checkoutRoot))
        Console.WriteLine("note: checkout has uncommitted changes.");

    Console.WriteLine();
    Console.WriteLine("This describes the INSTALLED tool, which is what a new leader launches.");
    Console.WriteLine("A leader that already runs keeps its old build until you restart it.");

    PrintPathHealthCheck();
    PrintUsage();
    return 0;
}

static string? GetCurrentBranch(string checkoutRoot)
{
    var (exitCode, stdout) = RunGitCapture(checkoutRoot, "rev-parse", "--abbrev-ref", "HEAD");
    if (exitCode != 0)
        return null;

    var branch = stdout.Trim();
    return branch is "" or "HEAD" ? null : branch;
}

static bool IsDirty(string checkoutRoot)
{
    var (exitCode, stdout) = RunGitCapture(checkoutRoot, "status", "--porcelain");
    return exitCode == 0 && stdout.Trim().Length > 0;
}

static bool IsToolsDirOnPath(string toolsDir)
{
    var pathVar = Environment.GetEnvironmentVariable("PATH") ?? "";
    var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    var target = NormalizePathEntry(toolsDir);
    return pathVar.Split(Path.PathSeparator)
        .Select(NormalizePathEntry)
        .Any(p => p is not null && string.Equals(p, target, comparison));
}

// The code normalizes PATH entries the same way a shell lookup does, then compares them.
// `/home/u/.dotnet/tools/`, `~/.dotnet/tools` and `/home/u//.dotnet/tools` all name the tools directory.
static string? NormalizePathEntry(string entry)
{
    var trimmed = entry.Trim();
    if (trimmed.Length == 0)
        return null;

    if (trimmed == "~" || trimmed.StartsWith("~/") || trimmed.StartsWith("~" + Path.DirectorySeparatorChar))
        trimmed = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), trimmed[1..]);

    try
    {
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(trimmed));
    }
    catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
    {
        return null;
    }
}

static string GetToolsDir() =>
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools");

static string GetShimPath() =>
    Path.Combine(GetToolsDir(), ToolCommandName + (OperatingSystem.IsWindows() ? ".exe" : ""));

static void PrintPathHealthCheck()
{
    var toolsDir = GetToolsDir();
    if (IsToolsDirOnPath(toolsDir))
        return;

    Console.WriteLine($"warning: {toolsDir} is not on PATH; the `{ToolCommandName}` command will not be found.");
    Console.WriteLine(OperatingSystem.IsWindows()
        ? $"  The .NET SDK installer normally adds this automatically. If not: setx PATH \"%PATH%;{toolsDir}\" (then re-open your terminal)"
        : $"  Add this to your shell profile (~/.bashrc, ~/.zshrc, etc.): export PATH=\"$PATH:{toolsDir}\"");
}

static string? ParseInstalledVersion(string toolListOutput)
{
    foreach (var line in toolListOutput.Split('\n'))
    {
        var columns = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (columns.Length >= 2 && columns[0].Equals(PackageId, StringComparison.OrdinalIgnoreCase))
            return columns[1];
    }
    return null;
}

static string? GetInstalledVersion()
{
    var (exitCode, stdout) = RunProcessCapture("dotnet", ["tool", "list", "--global"]);
    return exitCode == 0 ? ParseInstalledVersion(stdout) : null;
}

static (int ExitCode, string StdOut) RunGitCapture(string checkoutRoot, params string[] gitArgs)
{
    var psi = new ProcessStartInfo("git") { UseShellExecute = false, RedirectStandardOutput = true };
    psi.ArgumentList.Add("-C");
    psi.ArgumentList.Add(checkoutRoot);
    foreach (var a in gitArgs)
        psi.ArgumentList.Add(a);

    using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start git.");
    var stdout = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    return (process.ExitCode, stdout);
}

static (int ExitCode, string StdOut) RunProcessCapture(string executable, IEnumerable<string> arguments)
{
    var psi = new ProcessStartInfo(executable) { UseShellExecute = false, RedirectStandardOutput = true };
    foreach (var arg in arguments)
        psi.ArgumentList.Add(arg);

    using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {executable}.");
    var stdout = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    return (process.ExitCode, stdout);
}

static string FindCheckoutRoot([System.Runtime.CompilerServices.CallerFilePath] string scriptPath = "")
{
    var scriptsDir = Path.GetDirectoryName(Path.GetFullPath(scriptPath))
        ?? throw new InvalidOperationException($"Could not determine directory of script path: {scriptPath}");
    var root = Path.GetDirectoryName(scriptsDir)
        ?? throw new InvalidOperationException($"Could not determine checkout root above: {scriptsDir}");
    return root;
}

static int RunInstall(string checkoutRoot)
{
    if (IsDirty(checkoutRoot))
        Console.WriteLine("warning: checkout has uncommitted changes; the package is built from the working tree, so it includes them.");

    var packDir = Path.Combine(checkoutRoot, ".scratch", "nuget-local");
    Directory.CreateDirectory(packDir);
    foreach (var stale in Directory.GetFiles(packDir, "*.nupkg"))
        File.Delete(stale);

    var toolProject = Path.Combine(checkoutRoot, "src", "ClaudeTeam", "ClaudeTeam.csproj");
    var packExit = RunProcess("dotnet", ["pack", toolProject, "-c", "Release", "-o", packDir]);
    if (packExit != 0)
    {
        Console.Error.WriteLine($"pack failed (exit {packExit}).");
        return packExit;
    }

    var targetVersion = GetVersionFromNupkg(packDir);
    Console.WriteLine($"Packed version: {targetVersion}");

    var (localsExit, localsOut) = RunProcessCapture("dotnet", ["nuget", "locals", "global-packages", "--list"]);
    if (localsExit != 0)
    {
        Console.Error.WriteLine($"'dotnet nuget locals global-packages --list' failed (exit {localsExit}).");
        return localsExit;
    }

    var globalPackagesRoot = ParseNugetLocalsOutput(localsOut);
    var cachedVersionDir = Path.Combine(globalPackagesRoot, PackageId.ToLowerInvariant(), targetVersion.ToLowerInvariant());
    if (Directory.Exists(cachedVersionDir))
    {
        Directory.Delete(cachedVersionDir, recursive: true);
        Console.WriteLine($"Evicted stale cache entry: {cachedVersionDir}");
    }

    var installedVersion = GetInstalledVersion();
    int installExit;

    if (installedVersion is null || !installedVersion.Equals(targetVersion, StringComparison.OrdinalIgnoreCase))
    {
        // --allow-downgrade lets a checkout with a lower version replace a newer installed build.
        var updateArgs = new List<string> { "tool", "update", "--global", PackageId, "--add-source", packDir, "--version", targetVersion, "--allow-downgrade" };
        installExit = RunProcess("dotnet", updateArgs);
    }
    else
    {
        var uninstallArgs = new List<string> { "tool", "uninstall", "--global", PackageId };
        var uninstallExit = RunProcess("dotnet", uninstallArgs);

        if (uninstallExit != 0)
        {
            installExit = uninstallExit;
        }
        else
        {
            var installArgs = new List<string> { "tool", "install", "--global", PackageId, "--add-source", packDir, "--version", targetVersion };
            installExit = RunProcess("dotnet", installArgs);
        }
    }

    if (installExit != 0)
    {
        Console.Error.WriteLine($"Install failed (exit {installExit}).");
        return installExit;
    }

    PrintPathHealthCheck();

    var shimPath = GetShimPath();
    if (!File.Exists(shimPath))
    {
        Console.Error.WriteLine($"Install reported success but shim not found at: {shimPath}");
        return 1;
    }

    var (headExit, headOut) = RunGitCapture(checkoutRoot, "rev-parse", "--short", "HEAD");
    var branch = GetCurrentBranch(checkoutRoot);
    Console.WriteLine($"Installed {PackageId} {targetVersion} (commit {(headExit == 0 ? headOut.Trim() : "unknown")}, branch {branch ?? "detached HEAD"}).");
    Console.WriteLine($"Launch: {ToolCommandName}");
    Console.WriteLine("Restart a running leader to pick up this build.");
    return 0;
}

static int RunUninstall()
{
    var installedVersion = GetInstalledVersion();
    if (installedVersion is null)
    {
        Console.WriteLine($"{PackageId} is not installed as a global tool. Nothing to do.");
        return 0;
    }

    var exitCode = RunProcess("dotnet", ["tool", "uninstall", "--global", PackageId]);
    if (exitCode != 0)
    {
        Console.Error.WriteLine($"Uninstall failed (exit {exitCode}).");
        return exitCode;
    }

    Console.WriteLine($"Uninstalled {PackageId} {installedVersion}.");
    return 0;
}

static int RunReinstall(string checkoutRoot)
{
    var stopExit = StopRunningInstances();
    return stopExit != 0 ? stopExit : RunInstall(checkoutRoot);
}

static int StopRunningInstances()
{
    var shimPath = GetShimPath();

    List<(int Pid, string ExePath)> running;
    try
    {
        running = FindShimProcesses(shimPath);
    }
    catch (Exception ex)
    {
        return ReportStopFailure($"Could not enumerate running processes: {ex.Message}");
    }

    if (running.Count == 0)
    {
        Console.WriteLine($"No running {ToolCommandName} instances.");
        return 0;
    }

    var failed = false;
    foreach (var (pid, _) in running)
    {
        Console.WriteLine($"Stopping {ToolCommandName} (PID {pid})...");
        try
        {
            using var process = Process.GetProcessById(pid);
            process.Kill(entireProcessTree: false);

            if (process.WaitForExit(StopTimeoutMilliseconds))
            {
                Console.WriteLine("  stopped.");
            }
            else
            {
                Console.Error.WriteLine($"  PID {pid} did not exit within {StopTimeoutMilliseconds / 1000} seconds.");
                failed = true;
            }
        }
        catch (ArgumentException)
        {
            // The process exited between the scan and the kill. This is the result that we want.
            Console.WriteLine("  already exited.");
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
        {
            Console.Error.WriteLine($"  could not stop PID {pid}: {ex.Message}");
            failed = true;
        }
    }

    return failed ? ReportStopFailure($"One or more {ToolCommandName} instances are still running.") : 0;
}

// The platform decides if an instance that cannot stop blocks the install. This is not a policy choice.
// Windows holds an exclusive lock on the shim while it runs. If the install continues, it fails
// later with an unclear file-lock message. Unix lets the install replace the file of a running
// process, so there the only cost is a stale process.
static int ReportStopFailure(string message)
{
    if (OperatingSystem.IsWindows())
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine($"  Windows locks {GetShimPath()} while it runs, so the install cannot replace it.");
        return 1;
    }

    Console.WriteLine($"warning: {message}");
    Console.WriteLine("  Continuing: this platform can replace a running executable's file, so the install still applies.");
    Console.WriteLine("  Any instance left running keeps serving its old build until it is restarted.");
    return 0;
}

// The three platforms differ most in how they identify the executable of another process.
// Each platform uses its own native mechanism. One mechanism with fallback paths would be worse.
static List<(int Pid, string ExePath)> FindShimProcesses(string shimPath)
{
    if (OperatingSystem.IsWindows())
        return FindShimProcessesViaMainModule(shimPath);

    return OperatingSystem.IsLinux()
        ? FindShimProcessesViaProc(shimPath)
        : FindShimProcessesViaPs(shimPath);
}

static List<(int Pid, string ExePath)> FindShimProcessesViaMainModule(string shimPath)
{
    var found = new List<(int, string)>();

    foreach (var process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(shimPath)))
    {
        using (process)
        {
            string? exePath;
            try
            {
                exePath = process.MainModule?.FileName;
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                // Another user owns the process, it is elevated, or it ended after the enumeration.
                // Never assume that a process with an unreadable executable is ours.
                Console.WriteLine($"note: could not read the executable path of PID {process.Id}; skipping it.");
                continue;
            }

            if (exePath is not null && PathsEqual(exePath, shimPath))
                found.Add((process.Id, exePath));
        }
    }

    return found;
}

// This is not a name lookup on purpose. Linux truncates a process name to 15 characters, so a
// name match can find nothing and give no error. The `exe` link holds the full path and Linux
// does not truncate it.
static List<(int Pid, string ExePath)> FindShimProcessesViaProc(string shimPath)
{
    var found = new List<(int, string)>();

    foreach (var dir in Directory.EnumerateDirectories("/proc"))
    {
        if (!int.TryParse(Path.GetFileName(dir), out var pid))
            continue;

        string? exePath;
        try
        {
            exePath = File.ResolveLinkTarget(Path.Combine(dir, "exe"), returnFinalTarget: true)?.FullName;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The process belongs to another user, or it exited during the scan. It is not ours.
            continue;
        }

        if (exePath is not null && PathsEqual(exePath, shimPath))
            found.Add((pid, exePath));
    }

    return found;
}

// macOS does not let a process read the modules of another process, so MainModule is not available
// here. The code starts `ps` through ArgumentList with UseShellExecute = false, the same way as
// git and dotnet above. It starts a process, not a shell.
static List<(int Pid, string ExePath)> FindShimProcessesViaPs(string shimPath)
{
    var found = new List<(int, string)>();

    var (exitCode, stdout) = RunProcessCapture("ps", ["-Ao", "pid=,comm="]);
    if (exitCode != 0)
        throw new InvalidOperationException($"'ps -Ao pid=,comm=' failed (exit {exitCode}).");

    foreach (var line in stdout.Split('\n'))
    {
        var trimmed = line.Trim();
        var separator = trimmed.IndexOf(' ');
        if (separator < 0 || !int.TryParse(trimmed[..separator], out var pid))
            continue;

        // An executable path can contain spaces, so the path is all text after the first gap.
        var exePath = trimmed[(separator + 1)..].TrimStart();
        if (MatchesShim(exePath, shimPath))
            found.Add((pid, exePath));
    }

    return found;
}

// On macOS, the `comm` column holds the path of the executable. If it holds a bare command name,
// match the name and do not report nothing. A bare name has no directory to tell programs apart.
// This comparison is weaker, so the code uses it only when the stronger comparison cannot apply.
static bool MatchesShim(string candidate, string shimPath) =>
    PathsEqual(candidate, shimPath)
    || (!candidate.Contains('/') && string.Equals(candidate, Path.GetFileName(shimPath), StringComparison.Ordinal));

static bool PathsEqual(string left, string right)
{
    try
    {
        return string.Equals(
            Path.GetFullPath(left),
            Path.GetFullPath(right),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
    catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
    {
        return false;
    }
}

static string GetVersionFromNupkg(string packDir)
{
    var nupkgFiles = Directory.GetFiles(packDir, "*.nupkg");
    if (nupkgFiles.Length != 1)
        throw new InvalidOperationException($"Expected exactly one .nupkg in {packDir}, found {nupkgFiles.Length}.");

    var fileName = Path.GetFileName(nupkgFiles[0]);
    var match = Regex.Match(fileName, @"^claude-team\.(.+)\.nupkg$", RegexOptions.IgnoreCase);
    if (!match.Success)
        throw new InvalidOperationException($"Could not parse version from nupkg filename: {fileName}");
    return match.Groups[1].Value;
}

static string ParseNugetLocalsOutput(string output)
{
    var match = Regex.Match(output, @"global-packages:\s*(.+)", RegexOptions.IgnoreCase);
    if (!match.Success)
        throw new InvalidOperationException($"Could not parse 'dotnet nuget locals' output: {output}");
    return match.Groups[1].Value.Trim();
}

static int RunProcess(string executable, IEnumerable<string> arguments, string? workingDirectory = null)
{
    var psi = new ProcessStartInfo(executable) { UseShellExecute = false };
    if (workingDirectory is not null)
        psi.WorkingDirectory = workingDirectory;
    foreach (var arg in arguments)
        psi.ArgumentList.Add(arg);

    Console.WriteLine($"Running: {executable} {string.Join(' ', arguments)}");
    using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {executable}.");
    process.WaitForExit();
    return process.ExitCode;
}
#nullable restore
