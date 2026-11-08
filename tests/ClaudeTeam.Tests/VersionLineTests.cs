using ClaudeTeam;

namespace ClaudeTeam.Tests;

public class VersionLineTests
{
    [Fact]
    public void Shortens_the_commit_to_7_characters()
    {
        var full = "bf37b92" + new string('a', 33);
        Assert.Equal("claude-team 0.1.1-alpha.0.3+bf37b92", VersionLine.Format($"0.1.1-alpha.0.3+{full}"));
    }

    [Fact]
    public void Keeps_a_version_without_a_commit()
    {
        Assert.Equal("claude-team 0.2.0", VersionLine.Format("0.2.0"));
    }

    [Fact]
    public void Keeps_a_commit_that_is_already_short()
    {
        Assert.Equal("claude-team 0.2.0+abc", VersionLine.Format("0.2.0+abc"));
    }

    [Fact]
    public void Uses_unknown_when_the_version_is_missing()
    {
        Assert.Equal("claude-team unknown", VersionLine.Format(null));
    }
}
