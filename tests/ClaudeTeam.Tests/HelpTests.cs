using ClaudeTeam;

namespace ClaudeTeam.Tests;

public class HelpTests
{
    [Fact]
    public void Options_section_lists_the_version_option()
    {
        var text = Help.Text("tmux", "x");
        var options = text[text.IndexOf("Options:", StringComparison.Ordinal)..];
        Assert.Contains("  --version ", options);
    }
}
