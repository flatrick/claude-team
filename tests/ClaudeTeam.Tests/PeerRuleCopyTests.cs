using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class PeerRuleCopyTests
{
    [Fact]
    public void Repository_rule_copy_matches_the_embedded_source()
    {
        var root = FindRepositoryRoot();
        var copy = File.ReadAllText(PeerRule.RepoPath(root));
        Assert.Equal(PeerRule.Text, copy);
    }

    private static string FindRepositoryRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "ClaudeTeam.slnx")))
                return dir.FullName;
        throw new InvalidOperationException("ClaudeTeam.slnx not found above the test output directory.");
    }
}
