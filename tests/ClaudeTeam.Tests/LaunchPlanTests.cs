using ClaudeTeam;

namespace ClaudeTeam.Tests;

public class LaunchPlanTests
{
    private static LaunchInputs Inputs(bool windows = false, bool insidePane = false, bool hasAgent = true, string[]? forwarded = null)
        => new(Session: "claude-team", WorkingDirectory: "/src/claude team", IsWindows: windows,
            InsidePane: insidePane, HasTeamLeaderAgent: hasAgent, Forwarded: forwarded ?? []);

    [Fact]
    public void Outside_a_pane_on_linux_starts_an_attached_tmux_session_with_the_variable()
    {
        var plan = LaunchPlan.Build(Inputs());
        var command = Assert.Single(plan.Commands);
        Assert.Equal("tmux", command.File);
        Assert.Equal(
            ["new-session", "-s", "claude-team", "-c", "/src/claude team", "-e", "CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1",
             "claude", "--agent", "team-leader", "--teammate-mode", "tmux", "--name", "claude-team",
             "--append-system-prompt", "claude-team-leader"],
            command.Arguments);
        Assert.Equal("tmux", plan.Backend);
    }

    [Fact]
    public void Inside_a_pane_creates_a_detached_session_and_switches_to_it()
    {
        var plan = LaunchPlan.Build(Inputs(insidePane: true));
        Assert.Equal(2, plan.Commands.Count);
        Assert.Equal(["new-session", "-d", "-s", "claude-team", "-c", "/src/claude team"], plan.Commands[0].Arguments.Take(6));
        Assert.Equal(["switch-client", "-t", "=claude-team"], plan.Commands[1].Arguments);
    }

    [Fact]
    public void On_windows_uses_psmux_and_omits_the_working_directory()
    {
        var plan = LaunchPlan.Build(Inputs(windows: true));
        var command = Assert.Single(plan.Commands);
        Assert.Equal("psmux", command.File);
        Assert.DoesNotContain("-c", command.Arguments);
        Assert.DoesNotContain(command.Arguments, argument => argument.Contains(' '));
    }

    [Fact]
    public void Without_the_agent_file_drops_only_the_agent_flag()
    {
        var arguments = Assert.Single(LaunchPlan.Build(Inputs(hasAgent: false)).Commands).Arguments;
        Assert.DoesNotContain("--agent", arguments);
        Assert.Contains("--teammate-mode", arguments);
        Assert.Contains("--append-system-prompt", arguments);
        Assert.Contains("CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1", arguments);
    }

    [Fact]
    public void Forwarded_arguments_go_last()
    {
        var arguments = Assert.Single(LaunchPlan.Build(Inputs(forwarded: ["--model", "opus"])).Commands).Arguments;
        Assert.Equal(["--model", "opus"], arguments.TakeLast(2));
    }
}
