using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class TeamSkillsTests
{
    public static TheoryData<string> SkillNames => new() { "team-protocol", "start-teammate", "end-teammate" };

    [Theory]
    [MemberData(nameof(SkillNames))]
    public void Embedded_skill_text_has_lf_line_endings_only(string name)
    {
        var text = Installer.ReadEmbedded($"skills/{name}/SKILL.md");
        Assert.NotEmpty(text);
        Assert.DoesNotContain('\r', text);
    }
}
