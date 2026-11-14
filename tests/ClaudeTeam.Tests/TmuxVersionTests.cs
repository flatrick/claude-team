using ClaudeTeam;

namespace ClaudeTeam.Tests;

public class TmuxVersionTests
{
    [Theory]
    [InlineData("tmux 3.2")]
    [InlineData("tmux 3.2a")]
    [InlineData("tmux 3.8")]
    [InlineData("tmux 4.0")]
    [InlineData("tmux 10.1")]
    public void Supported_versions_pass(string text)
        => Assert.True(TmuxVersion.IsSupported(text));

    [Theory]
    [InlineData("tmux 3.1c")]
    [InlineData("tmux 3.0")]
    [InlineData("tmux 2.9a")]
    public void Old_versions_fail(string text)
        => Assert.False(TmuxVersion.IsSupported(text));

    [Theory]
    [InlineData("tmux next-3.4")]
    [InlineData("tmux master")]
    [InlineData("")]
    public void Unknown_text_continues(string text)
    {
        Assert.False(TmuxVersion.TryParse(text, out _, out _));
        Assert.True(TmuxVersion.IsSupported(text));
    }

    [Fact]
    public void TryParse_returns_major_and_minor()
    {
        Assert.True(TmuxVersion.TryParse("tmux 3.1c", out var major, out var minor));
        Assert.Equal((3, 1), (major, minor));
    }

    [Fact]
    public void Message_names_the_found_version_and_the_minimum()
    {
        var message = TmuxVersion.TooOldMessage("tmux 3.1c");
        Assert.Contains("3.1c", message);
        Assert.Contains($"{TmuxVersion.MinimumMajor}.{TmuxVersion.MinimumMinor}", message);
    }
}
