using ClaudeTeam;

namespace ClaudeTeam.Tests;

public class ShippedHashesTests
{
    [Fact]
    public void Every_shipped_file_has_the_hash_of_its_current_text_in_the_list()
    {
        foreach (var file in ShippedFiles.All)
            Assert.True(
                ShippedHashes.Contains(file.PublishedPath, file.Text),
                $"shipped-hashes.txt lacks the hash of the current text of {file.PublishedPath}: {ShippedHashes.Sha256(file.Text)}");
    }

    [Fact]
    public void Shipped_files_are_the_rule_the_agent_and_the_skills_in_install_order()
        => Assert.Equal(
            [
                "rules/claude-team-peer-leaders.md",
                "agents/team-leader.md",
                "skills/team-protocol/SKILL.md",
                "skills/start-teammate/SKILL.md",
                "skills/end-teammate/SKILL.md",
            ],
            ShippedFiles.All.Select(file => file.PublishedPath).ToArray());

    [Fact]
    public void A_hash_belongs_to_one_published_path()
    {
        var text = ShippedFiles.All[0].Text;
        Assert.False(ShippedHashes.Contains("agents/team-leader.md", text));
    }

    [Fact]
    public void Line_endings_do_not_change_the_hash()
        => Assert.Equal(ShippedHashes.Sha256("a\nb\n"), ShippedHashes.Sha256("a\r\nb\r\n"));
}
