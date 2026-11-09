using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class LeaderAgentTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("claude-team-tests-");
    private string Repo => Path.Combine(_root.FullName, "repo");
    private string Home => Path.Combine(_root.FullName, "home");

    public void Dispose() => _root.Delete(recursive: true);

    [Fact]
    public void Written_file_has_lf_line_endings_only()
    {
        LeaderAgent.Install(InstallTarget.Repo, Repo, Home, force: false);
        var bytes = File.ReadAllBytes(Path.Combine(Repo, ".claude", "agents", LeaderAgent.FileName));
        Assert.DoesNotContain((byte)'\r', bytes);
    }

    [Fact]
    public void Dry_run_install_writes_nothing()
    {
        var result = Assert.Single(LeaderAgent.Install(InstallTarget.Global, Repo, Home, force: false, dryRun: true));
        Assert.Equal(InstallOutcome.Written, result.Outcome);
        Assert.False(File.Exists(LeaderAgent.GlobalPath(Home)));
    }

    [Fact]
    public void Is_missing_when_neither_file_exists()
        => Assert.False(LeaderAgent.IsInstalled(Repo, Home));

    [Fact]
    public void Install_both_writes_both_files_and_then_counts_as_installed()
    {
        var results = LeaderAgent.Install(InstallTarget.Both, Repo, Home, force: false);
        Assert.All(results, result => Assert.Equal(InstallOutcome.Written, result.Outcome));
        Assert.Equal(LeaderAgent.Text, File.ReadAllText(Path.Combine(Repo, ".claude", "agents", LeaderAgent.FileName)));
        Assert.Equal(LeaderAgent.Text, File.ReadAllText(Path.Combine(Home, ".claude", "agents", LeaderAgent.FileName)));
        Assert.True(LeaderAgent.IsInstalled(Repo, Home));
    }

    [Fact]
    public void Either_location_alone_counts_as_installed()
    {
        LeaderAgent.Install(InstallTarget.Global, Repo, Home, force: false);
        Assert.True(LeaderAgent.IsInstalled(Repo, Home));
        Assert.True(LeaderAgent.IsInstalled(null, Home));
    }

    [Fact]
    public void Install_keeps_an_existing_file_without_force()
    {
        var path = Path.Combine(Repo, ".claude", "agents", LeaderAgent.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "edited");

        var result = Assert.Single(LeaderAgent.Install(InstallTarget.Repo, Repo, Home, force: false));
        Assert.Equal(InstallOutcome.KeptExisting, result.Outcome);
        Assert.Equal("edited", File.ReadAllText(path));

        LeaderAgent.Install(InstallTarget.Repo, Repo, Home, force: true);
        Assert.Equal(LeaderAgent.Text, File.ReadAllText(path));
    }

    [Fact]
    public void Install_to_repo_fails_outside_git()
    {
        var result = Assert.Single(LeaderAgent.Install(InstallTarget.Repo, null, Home, force: false));
        Assert.Equal(InstallOutcome.NoRepository, result.Outcome);
    }
}
