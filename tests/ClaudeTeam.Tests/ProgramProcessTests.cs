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
}
