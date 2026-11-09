using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class ShippedContentTests
{
    private static readonly string[] Forbidden =
    [
        "session-coordination", ".scratch", ".worktrees", "dotnet", "decisions.md", "NOT RE-DERIVED",
        "friction-scribe", "preset", "test-runner", "implementer-", "requirements-", "troubleshooter-",
    ];

    public static TheoryData<string> OwnNames => new() { "team-leader", "team-protocol", "start-teammate", "end-teammate" };

    private static string TextOf(string name) => name == "team-leader" ? LeaderAgent.Text : TeamSkills.Text(name);

    private static (string FrontMatter, string Body) Split(string text)
    {
        Assert.StartsWith("---\n", text);
        var end = text.IndexOf("\n---\n", 3, StringComparison.Ordinal);
        Assert.True(end > 0, "The front matter has no closing line.");
        return (text[4..end], text[(end + 5)..]);
    }

    private static string Body(string name) => Split(TextOf(name)).Body;

    [Theory]
    [MemberData(nameof(OwnNames))]
    public void First_body_line_invokes_the_local_skill(string name)
    {
        var first = Body(name).Split('\n').First(line => line.Trim().Length > 0);
        Assert.Equal($"Invoke the skill `{name}-local`. If the tool says `Unknown skill`, continue.", first);
    }

    [Theory]
    [MemberData(nameof(OwnNames))]
    public void Shipped_text_has_no_repository_conventions(string name)
    {
        var text = TextOf(name);
        foreach (var forbidden in Forbidden)
            Assert.DoesNotContain(forbidden, text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Agent_front_matter_has_the_required_fields()
    {
        var (frontMatter, _) = Split(LeaderAgent.Text);
        var lines = frontMatter.Split('\n');
        Assert.Equal("name: team-leader", lines[0]);
        Assert.Contains("effort: low", lines);
        Assert.Contains("memory: local", lines);
        Assert.Contains(lines, line => line.StartsWith("description:"));
        Assert.Contains(lines, line => line.StartsWith("initialPrompt:") && line.Contains("team-protocol"));
        Assert.DoesNotContain(lines, line => line.StartsWith("model:"));
    }

    [Fact]
    public void Agent_body_invokes_team_protocol_after_the_local_line()
    {
        var lines = Body("team-leader").Split('\n').Where(line => line.Trim().Length > 0).ToList();
        Assert.Contains("team-protocol", lines[1]);
    }

    [Fact]
    public void Agent_confirms_its_own_load_to_the_operator()
    {
        Assert.Contains("`team-leader loaded`", LeaderAgent.Text);
        Assert.Contains("`team-leader loaded +local`", LeaderAgent.Text);
    }

    [Fact]
    public void Agent_uses_the_start_and_end_skills_and_repeats_no_procedure()
    {
        Assert.Contains("`start-teammate`", LeaderAgent.Text);
        Assert.Contains("`end-teammate`", LeaderAgent.Text);
        Assert.DoesNotContain("tmux list-panes", LeaderAgent.Text);
    }

    [Fact]
    public void Agent_keeps_long_running_roles_out_of_subagents()
        => Assert.Contains(
            "Never start a long-running role as a subagent without the approval of the operator.", LeaderAgent.Text);

    [Fact]
    public void Protocol_gives_both_load_lines_and_the_local_skill_location()
    {
        var text = TeamSkills.Text("team-protocol");
        Assert.Contains("`team-protocol loaded`", text);
        Assert.Contains("`team-protocol loaded +local`", text);
        Assert.Contains("in the `.claude/skills/` folder of the repository", text);
    }

    [Fact]
    public void Protocol_takes_report_path_model_and_count_from_the_assignment()
    {
        var text = TeamSkills.Text("team-protocol");
        Assert.Contains("creates no subagent", text);
        Assert.Contains("default model of the session", text);
        Assert.Contains("the base commit", text);
        Assert.DoesNotContain("Sonnet", text);
        Assert.DoesNotContain("Opus", text);
        Assert.Contains("agent type", text);
    }

    [Fact]
    public void Start_skill_checks_the_local_load_of_a_teammate()
    {
        var text = TeamSkills.Text("start-teammate");
        Assert.Contains("`team-protocol loaded +local`", text);
        Assert.Contains("team-protocol-local", text);
        Assert.Contains("A missing line is not a launch failure.", text);
    }

    [Fact]
    public void Start_skill_gives_psmux_and_powershell_forms()
    {
        var text = TeamSkills.Text("start-teammate");
        Assert.Contains("tmux list-panes -a -F '#{pane_id} #{pane_title}'", text);
        Assert.Contains("psmux list-panes -a -F '#{pane_id} #{pane_title}'", text);
        Assert.Contains("not tested on Windows", text);
        Assert.Contains("$env:CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS", text);
        AssertChecksDecide(text);
    }

    [Fact]
    public void End_skill_gives_psmux_and_powershell_forms()
    {
        var text = TeamSkills.Text("end-teammate");
        Assert.Contains("psmux list-panes -a -F '#{pane_id} #{pane_title}'", text);
        Assert.Contains("not tested on Windows", text);
        Assert.Contains("Get-Date", text);
        Assert.Contains("Start-Sleep", text);
        AssertChecksDecide(text);
    }

    [Theory]
    [MemberData(nameof(OwnNames))]
    public void Shipped_text_never_tells_the_user_to_put_a_local_skill_in_the_user_profile(string name)
    {
        var sentences = TextOf(name).Split(['\n', '.'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var sentence in sentences.Where(s => s.Contains("-local")
                     && (s.Contains("user profile", StringComparison.OrdinalIgnoreCase) || s.Contains("~/.claude/skills"))))
            Assert.StartsWith("Do not", sentence);
    }

    [Fact]
    public void Protocol_forbids_a_local_skill_in_the_user_profile_and_says_why()
    {
        var text = TeamSkills.Text("team-protocol");
        Assert.Contains("Do not put a `-local` skill in the user profile.", text);
        Assert.Contains("A user profile skill hides a repository skill of the same name.", text);
    }

    [Fact]
    public void Start_skill_sends_one_reminder_and_then_tells_the_operator_when_the_local_line_is_missing()
    {
        var text = TeamSkills.Text("start-teammate");
        var section = text[text.IndexOf("## Confirm the protocol load", StringComparison.Ordinal)..];
        var reminder = section.IndexOf("send the teammate one message", StringComparison.Ordinal);
        var report = section.IndexOf("still lacks the line, tell the operator", StringComparison.Ordinal);

        Assert.True(reminder >= 0, "The section has no reminder step.");
        Assert.Equal(reminder, section.LastIndexOf("send the teammate one message", StringComparison.Ordinal));
        Assert.True(report > reminder, "The report to the operator must come after the reminder.");
        Assert.Contains("lacks `+local` when you need it", section);
        Assert.Contains("If you loaded `team-protocol-local` yourself, the line must be `team-protocol loaded +local`.", section);
        Assert.Contains("A missing line is not a launch failure.", section);
    }

    private static void AssertChecksDecide(string text)
    {
        Assert.Contains("psmux can behave differently from tmux and give no error", text);
        Assert.Contains("never by the exit code of a multiplexer command", text);
    }
}
