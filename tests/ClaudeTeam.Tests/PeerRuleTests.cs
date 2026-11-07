using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class PeerRuleTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("claude-team-tests-");
    private string Repo => Path.Combine(_root.FullName, "repo");
    private string Home => Path.Combine(_root.FullName, "home");

    public void Dispose() => _root.Delete(recursive: true);

    [Fact]
    public void Embedded_text_contains_the_marker()
        => Assert.Contains(PeerRule.Marker, PeerRule.Text);

    [Fact]
    public void Is_missing_when_neither_file_exists()
        => Assert.False(PeerRule.IsInstalled(Repo, Home));

    [Fact]
    public void Install_both_writes_both_files_and_then_counts_as_installed()
    {
        var results = PeerRule.Install(InstallTarget.Both, Repo, Home, force: false);
        Assert.All(results, result => Assert.Equal(InstallOutcome.Written, result.Outcome));
        Assert.Equal(PeerRule.Text, File.ReadAllText(Path.Combine(Repo, ".claude", "rules", PeerRule.FileName)));
        Assert.Equal(PeerRule.Text, File.ReadAllText(Path.Combine(Home, ".claude", "rules", PeerRule.FileName)));
        Assert.True(PeerRule.IsInstalled(Repo, Home));
    }

    [Fact]
    public void Either_location_alone_counts_as_installed()
    {
        PeerRule.Install(InstallTarget.Global, Repo, Home, force: false);
        Assert.True(PeerRule.IsInstalled(Repo, Home));
        Assert.True(PeerRule.IsInstalled(null, Home));
    }

    [Fact]
    public void Install_keeps_an_existing_file_without_force()
    {
        var path = Path.Combine(Repo, ".claude", "rules", PeerRule.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "edited");

        var result = Assert.Single(PeerRule.Install(InstallTarget.Repo, Repo, Home, force: false));
        Assert.Equal(InstallOutcome.KeptExisting, result.Outcome);
        Assert.Equal("edited", File.ReadAllText(path));

        PeerRule.Install(InstallTarget.Repo, Repo, Home, force: true);
        Assert.Equal(PeerRule.Text, File.ReadAllText(path));
    }

    [Fact]
    public void Install_to_repo_fails_outside_git()
    {
        var result = Assert.Single(PeerRule.Install(InstallTarget.Repo, null, Home, force: false));
        Assert.Equal(InstallOutcome.NoRepository, result.Outcome);
    }
}
