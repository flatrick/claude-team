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

    [Fact]
    public void Leader_agent_option_names_the_three_skills()
    {
        var text = Help.Text("tmux", "x");
        var options = text[text.IndexOf("Options:", StringComparison.Ordinal)..];
        var start = options.IndexOf("--install-leader-agent <target>", StringComparison.Ordinal);
        var end = options.IndexOf("--force", start, StringComparison.Ordinal);
        var section = options[start..end];
        foreach (var name in TeamSkills.Names)
            Assert.Contains(name, section);
    }
}
