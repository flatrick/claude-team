using System.Diagnostics;
using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class ProgramProcessTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("claude-team-tests-");

    public void Dispose() => _root.Delete(recursive: true);

    private (int ExitCode, string Output) Run(params string[] args)
    {
        var work = Directory.CreateDirectory(Path.Combine(_root.FullName, "work")).FullName;
        var home = Directory.CreateDirectory(Path.Combine(_root.FullName, "home")).FullName;
        var info = new ProcessStartInfo("dotnet") { WorkingDirectory = work, RedirectStandardOutput = true, RedirectStandardError = true };
        info.ArgumentList.Add(typeof(Options).Assembly.Location);
        foreach (var arg in args)
            info.ArgumentList.Add(arg);
        info.Environment["HOME"] = home;
        info.Environment["USERPROFILE"] = home;
        info.Environment.Remove("TMUX");
        using var process = Process.Start(info)!;
        var output = process.StandardOutput.ReadToEnd();
        process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output);
    }

    [Fact]
    public void Help_with_update_prints_help_writes_no_file_and_exits_0()
    {
        var (exitCode, output) = Run("--help", "--update", "both");

        Assert.Equal(0, exitCode);
        Assert.StartsWith("Starts a Claude team leader", output);
        Assert.Empty(Directory.GetFileSystemEntries(Path.Combine(_root.FullName, "work")));
        Assert.Empty(Directory.GetFileSystemEntries(Path.Combine(_root.FullName, "home")));
    }

    [Fact]
    public void Update_dry_run_prints_the_changes_and_writes_no_file()
    {
        var (exitCode, output) = Run("--update", "global", "--dry-run");

        Assert.Equal(0, exitCode);
        Assert.Contains("Would add ", output);
        Assert.Empty(Directory.GetFileSystemEntries(Path.Combine(_root.FullName, "home")));
    }

    // Writes a fake tmux that logs each call, prints versionText for -V, and exits 1 for has-session.
    private (int ExitCode, string Error, string[] Calls) RunWithFakeTmux(string versionOutput, int versionExit, params string[] args)
    {
        var bin = Directory.CreateDirectory(Path.Combine(_root.FullName, "bin")).FullName;
        var log = Path.Combine(_root.FullName, "tmux-calls.log");
        var script = Path.Combine(bin, "tmux");
        File.WriteAllText(script, $"""
            #!/bin/sh
            echo "$*" >> '{log}'
            case "$1" in
              -V) echo '{versionOutput}'; exit {versionExit} ;;
              has-session) exit 1 ;;
            esac
            exit 0
            """.Replace("\r\n", "\n"));
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(script, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

        var work = Directory.CreateDirectory(Path.Combine(_root.FullName, "work")).FullName;
        var home = Directory.CreateDirectory(Path.Combine(_root.FullName, "home")).FullName;
        var info = new ProcessStartInfo("dotnet") { WorkingDirectory = work, RedirectStandardOutput = true, RedirectStandardError = true };
        info.ArgumentList.Add(typeof(Options).Assembly.Location);
        foreach (var arg in args)
            info.ArgumentList.Add(arg);
        info.Environment["HOME"] = home;
        info.Environment["USERPROFILE"] = home;
        info.Environment["PATH"] = bin + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH");
        info.Environment.Remove("TMUX");
        using var process = Process.Start(info)!;
        process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, error, File.Exists(log) ? File.ReadAllLines(log) : []);
    }

    [Fact]
    public void Old_tmux_stops_the_launch_with_exit_1_and_names_the_version()
    {
        if (OperatingSystem.IsWindows())
            return;

        var (exitCode, error, calls) = RunWithFakeTmux("tmux 3.1c", 0, "--session", "mux-test");

        Assert.Equal(1, exitCode);
        Assert.Contains("3.1c", error);
        Assert.Contains("3.2", error);
        Assert.DoesNotContain(calls, call => call.StartsWith("new-session"));
    }

    [Fact]
    public void Supported_tmux_continues_to_new_session()
    {
        if (OperatingSystem.IsWindows())
            return;

        var (exitCode, _, calls) = RunWithFakeTmux("tmux 3.2a", 0, "--session", "mux-test");

        Assert.Equal(0, exitCode);
        Assert.Contains(calls, call => call.StartsWith("new-session"));
    }

    [Fact]
    public void Unknown_version_text_continues_to_new_session()
    {
        if (OperatingSystem.IsWindows())
            return;

        var (exitCode, _, calls) = RunWithFakeTmux("tmux next-3.4", 0, "--session", "mux-test");

        Assert.Equal(0, exitCode);
        Assert.Contains(calls, call => call.StartsWith("new-session"));
    }

    [Fact]
    public void Failed_version_command_continues_to_new_session()
    {
        if (OperatingSystem.IsWindows())
            return;

        var (exitCode, _, calls) = RunWithFakeTmux("tmux 3.1c", 1, "--session", "mux-test");

        Assert.Equal(0, exitCode);
        Assert.Contains(calls, call => call.StartsWith("new-session"));
    }

    [Fact]
    public void Dry_run_does_not_run_tmux()
    {
        if (OperatingSystem.IsWindows())
            return;

        var (exitCode, _, calls) = RunWithFakeTmux("tmux 3.1c", 0, "--session", "mux-test", "--dry-run");

        Assert.Equal(0, exitCode);
        Assert.Empty(calls);
    }
}
