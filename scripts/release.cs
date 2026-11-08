#!/usr/bin/env dotnet
#:property PublishAot=false
#nullable enable

// Prepare and tag a release of claude-team. The script never pushes.
//
// Run the script from any directory. The script finds the checkout from its own file path.
// Pass the mode after `--`:
//   dotnet run --file scripts/release.cs -- prepare                       # on a branch: write CHANGELOG.md and commit it
//   dotnet run --file scripts/release.cs -- prepare --set-version 1.0.0   # use this version, not the computed one
//   dotnet run --file scripts/release.cs -- tag                           # on main, after the merge: create the tag
//
// `prepare` needs `git-cliff` on PATH. While the major version is 0, it asks git-cliff for a minor bump.
// `tag` runs `git fetch origin` and makes the tag only on the commit that is on origin/main.
// The operator pushes the tag with the command that `tag` prints.

using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

var checkoutRoot = FindCheckoutRoot();

return args switch
{
    ["prepare"] => RunPrepare(checkoutRoot, null),
    ["prepare", "--set-version", var version] => RunPrepare(checkoutRoot, version),
    ["tag"] => RunTag(checkoutRoot),
    _ => Usage()
};

static int Usage()
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  dotnet run --file scripts/release.cs -- prepare [--set-version <X.Y.Z>]");
    Console.Error.WriteLine("  dotnet run --file scripts/release.cs -- tag");
    return 1;
}

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}

static int RunPrepare(string root, string? setVersion)
{
    if (!Run(root, "git-cliff", ["--version"], out _, out _))
        return Fail("git-cliff is not on PATH. Install git-cliff and try again.");

    if (!Git(root, ["status", "--porcelain"], out var status) || status.Trim().Length > 0)
        return Fail("The working tree has uncommitted changes. Commit or discard them first.");

    if (!Git(root, ["symbolic-ref", "--short", "-q", "HEAD"], out var branchOut))
        return Fail("The checkout is on a detached HEAD. Switch to a branch first.");
    if (branchOut.Trim() == "main")
        return Fail("The checkout is on the main branch. Run prepare on a branch.");

    if (!Git(root, ["describe", "--tags", "--abbrev=0", "--match", "v*"], out var tagOut))
        return Fail("No tag with the prefix v exists.");
    var latestTag = tagOut.Trim();
    var latest = ParseVersion(latestTag.TrimStart('v'));
    if (latest is null)
        return Fail($"The latest tag '{latestTag}' is not of the form vX.Y.Z.");

    if (!Git(root, ["rev-list", "--count", $"{latestTag}..HEAD"], out var countOut) || countOut.Trim() == "0")
        return Fail($"No commits follow the latest tag {latestTag}. Nothing to release.");

    string next;
    if (setVersion is not null)
    {
        var requested = ParseVersion(setVersion);
        if (requested is null)
            return Fail($"The version '{setVersion}' is not of the form X.Y.Z.");
        if (requested.Value.CompareTo(latest.Value) <= 0)
            return Fail($"The version {setVersion} is not higher than the latest tag {latestTag}.");
        // git-cliff takes only major, minor, patch, or auto for --bump, so the script passes the explicit version as the tag.
        next = $"v{setVersion}";
    }
    else
    {
        var bumpArguments = latest.Value.Major == 0 ? new[] { "--bump", "minor" } : ["--bump"];
        if (!Run(root, "git-cliff", [.. bumpArguments, "--bumped-version"], out var versionOut, out var versionErr))
            return Fail($"git-cliff could not compute the next version.{Environment.NewLine}{versionErr}");
        next = versionOut.Trim();
        if (ParseVersion(next.TrimStart('v')) is null)
            return Fail($"git-cliff gave an unexpected version: '{next}'.");
    }

    if (!Run(root, "git-cliff", ["--tag", next, "-o", "CHANGELOG.md"], out _, out var changelogErr))
        return Fail($"git-cliff could not write CHANGELOG.md.{Environment.NewLine}{changelogErr}");

    const string restoreHint = "CHANGELOG.md has changes that are not committed. Run git status. Restore or delete CHANGELOG.md before you run prepare again.";
    if (!Git(root, ["add", "CHANGELOG.md"], out _))
        return Fail($"git add failed for CHANGELOG.md.{Environment.NewLine}{restoreHint}");
    if (!Run(root, "git", ["commit", "-m", $"chore(release): {next}"], out _, out var commitErr))
        return Fail($"git commit failed.{Environment.NewLine}{commitErr}{Environment.NewLine}{restoreHint}");

    Console.WriteLine($"Prepared release {next}. CHANGELOG.md is committed.");
    Console.WriteLine("Open a pull request and use Rebase and merge. Then run the tag mode on main.");
    return 0;
}

static int RunTag(string root)
{
    if (!Git(root, ["symbolic-ref", "--short", "-q", "HEAD"], out var branchOut) || branchOut.Trim() != "main")
        return Fail("The checkout is not on the main branch. Run tag on main.");

    if (!Git(root, ["status", "--porcelain"], out var status) || status.Trim().Length > 0)
        return Fail("The working tree has uncommitted changes. Commit or discard them first.");

    if (!Run(root, "git", ["fetch", "origin"], out _, out var fetchErr))
        return Fail($"git fetch origin failed.{Environment.NewLine}{fetchErr}");

    if (!Git(root, ["rev-parse", "HEAD"], out var head) || !Git(root, ["rev-parse", "origin/main"], out var origin))
        return Fail("Cannot read HEAD or origin/main.");
    if (head.Trim() != origin.Trim())
        return Fail("HEAD is not the same commit as origin/main. Run git pull --ff-only, or push your commits first.");

    if (!Git(root, ["log", "-1", "--format=%s"], out var subjectOut))
        return Fail("Cannot read the subject of HEAD.");
    var subject = subjectOut.Trim();
    var match = Regex.Match(subject, @"^chore\(release\): v([0-9]+\.[0-9]+\.[0-9]+)$");
    if (!match.Success)
        return Fail($"The subject of HEAD is '{subject}'. It must be 'chore(release): vX.Y.Z'.");

    var version = match.Groups[1].Value;
    var tag = $"v{version}";
    if (Git(root, ["rev-parse", "-q", "--verify", $"refs/tags/{tag}"], out _))
        return Fail($"The tag {tag} exists already.");

    if (!Run(root, "git", ["tag", "-a", tag, "-m", $"claude-team {version}"], out _, out var tagErr))
        return Fail($"git tag failed.{Environment.NewLine}{tagErr}");

    Console.WriteLine($"Created the tag {tag} on HEAD. Push it with:");
    Console.WriteLine($"git push origin {tag}");
    return 0;
}

static (int Major, int Minor, int Patch)? ParseVersion(string text)
{
    var match = Regex.Match(text, @"^([0-9]+)\.([0-9]+)\.([0-9]+)$");
    if (match.Success
        && int.TryParse(match.Groups[1].Value, out var major)
        && int.TryParse(match.Groups[2].Value, out var minor)
        && int.TryParse(match.Groups[3].Value, out var patch))
        return (major, minor, patch);
    return null;
}

static bool Git(string root, string[] gitArguments, out string stdout) =>
    Run(root, "git", gitArguments, out stdout, out _);

// Starts the child process without a shell. Returns true when the exit code is 0.
static bool Run(string root, string executable, string[] arguments, out string stdout, out string stderr)
{
    var psi = new ProcessStartInfo(executable)
    {
        WorkingDirectory = root,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    foreach (var argument in arguments)
        psi.ArgumentList.Add(argument);

    try
    {
        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {executable}.");
        var errorTask = process.StandardError.ReadToEndAsync();
        stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        stderr = errorTask.Result;
        return process.ExitCode == 0;
    }
    catch (Win32Exception)
    {
        stdout = "";
        stderr = $"Cannot start {executable}.";
        return false;
    }
}

static string FindCheckoutRoot([System.Runtime.CompilerServices.CallerFilePath] string scriptPath = "")
{
    var scriptsDir = Path.GetDirectoryName(Path.GetFullPath(scriptPath))
        ?? throw new InvalidOperationException($"Could not determine directory of script path: {scriptPath}");
    return Path.GetDirectoryName(scriptsDir)
        ?? throw new InvalidOperationException($"Could not determine checkout root above: {scriptsDir}");
}
