using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class TeamSkillsTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("claude-team-tests-");
    private string Repo => Path.Combine(_root.FullName, "repo");
    private string Home => Path.Combine(_root.FullName, "home");

    public void Dispose() => _root.Delete(recursive: true);

    public static TheoryData<string> SkillNames => new() { "team-protocol", "start-teammate", "end-teammate" };

    [Theory]
    [MemberData(nameof(SkillNames))]
    public void Embedded_skill_text_has_lf_line_endings_only(string name)
    {
        var text = Installer.ReadEmbedded($"skills/{name}/SKILL.md");
        Assert.NotEmpty(text);
        Assert.DoesNotContain('\r', text);
    }

    private static void Put(string path, string text = "x")
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    [Fact]
    public void Warns_once_and_names_every_missing_skill_in_install_order()
    {
        var warning = TeamSkills.MissingWarning(Repo, Home);
        Assert.Equal(
            "Warning: the team skills team-protocol, start-teammate, end-teammate are not in .claude/skills/ or ~/.claude/skills/. " +
            "Install them with: claude-team --install-leader-agent <repo|global|both>",
            warning);
    }

    [Fact]
    public void Names_only_the_missing_skill()
    {
        Put(TeamSkills.RepoPath(Repo, "team-protocol"));
        Put(TeamSkills.GlobalPath(Home, "start-teammate"));
        var warning = TeamSkills.MissingWarning(Repo, Home);
        Assert.NotNull(warning);
        Assert.Contains("end-teammate", warning);
        Assert.DoesNotContain("team-protocol", warning);
        Assert.DoesNotContain("start-teammate", warning);
        Assert.Contains("--install-leader-agent", warning);
    }

    [Fact]
    public void No_warning_when_each_skill_is_in_at_least_one_location()
    {
        Put(TeamSkills.RepoPath(Repo, "team-protocol"));
        Put(TeamSkills.GlobalPath(Home, "start-teammate"));
        Put(TeamSkills.RepoPath(Repo, "end-teammate"));
        Put(TeamSkills.GlobalPath(Home, "end-teammate"));
        Assert.Null(TeamSkills.MissingWarning(Repo, Home));
    }

    [Fact]
    public void No_warning_when_two_copies_differ()
    {
        foreach (var name in TeamSkills.Names)
        {
            Put(TeamSkills.RepoPath(Repo, name), "one");
            Put(TeamSkills.GlobalPath(Home, name), "two");
        }
        Assert.Null(TeamSkills.MissingWarning(Repo, Home));
    }
}
