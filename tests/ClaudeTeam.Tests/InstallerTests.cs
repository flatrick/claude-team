using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class InstallerTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("claude-team-tests-");
    private string Repo => Path.Combine(_root.FullName, "repo");
    private string Home => Path.Combine(_root.FullName, "home");

    public void Dispose() => _root.Delete(recursive: true);

    [Fact]
    public void Combined_run_writes_rule_and_agent()
    {
        var run = Installer.Run(peerRule: InstallTarget.Repo, leaderAgent: InstallTarget.Global, repoRoot: Repo, home: Home, force: false, dryRun: false);
        Assert.Equal(0, run.ExitCode);
        Assert.Equal(
            [$"Wrote {PeerRule.RepoPath(Repo)}", $"Wrote {LeaderAgent.GlobalPath(Home)}"],
            run.Lines);
        Assert.True(File.Exists(PeerRule.RepoPath(Repo)));
        Assert.True(File.Exists(LeaderAgent.GlobalPath(Home)));
    }

    [Fact]
    public void Outside_git_reports_the_path_and_exits_1()
    {
        var run = Installer.Run(peerRule: null, leaderAgent: InstallTarget.Repo, repoRoot: null, home: Home, force: false, dryRun: false);
        Assert.Equal(1, run.ExitCode);
        var line = Assert.Single(run.Lines);
        Assert.Contains(Path.Combine(".claude", "agents", LeaderAgent.FileName), line);
        Assert.Contains("not in a git repository", line);
    }

    [Fact]
    public void Dry_run_writes_nothing_and_reports_what_would_happen()
    {
        var existing = PeerRule.GlobalPath(Home);
        Directory.CreateDirectory(Path.GetDirectoryName(existing)!);
        File.WriteAllText(existing, "edited");

        var run = Installer.Run(peerRule: InstallTarget.Both, leaderAgent: InstallTarget.Both, repoRoot: Repo, home: Home, force: false, dryRun: true);

        Assert.Equal(0, run.ExitCode);
        Assert.Contains($"Would write {PeerRule.RepoPath(Repo)}", run.Lines);
        Assert.Contains($"Would write {LeaderAgent.GlobalPath(Home)}", run.Lines);
        Assert.Contains(run.Lines, line => line.StartsWith($"Would keep the existing {existing}"));
        Assert.False(Directory.Exists(Repo));
        Assert.False(File.Exists(LeaderAgent.GlobalPath(Home)));
        Assert.Equal("edited", File.ReadAllText(existing));
    }

    [Fact]
    public void Dry_run_outside_git_still_exits_1()
    {
        var run = Installer.Run(peerRule: InstallTarget.Repo, leaderAgent: null, repoRoot: null, home: Home, force: false, dryRun: true);
        Assert.Equal(1, run.ExitCode);
    }
}
