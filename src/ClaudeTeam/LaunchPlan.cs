namespace ClaudeTeam;

public sealed record LaunchInputs(
    string Session,
    string WorkingDirectory,
    bool IsWindows,
    bool InsidePane,
    bool HasTeamLeaderAgent,
    string[] Forwarded);

public sealed record Command(string File, IReadOnlyList<string> Arguments);

public sealed record LaunchPlan(string Backend, IReadOnlyList<Command> Commands)
{
    public const string TeamsVariable = "CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS";

    public static string BackendFor(bool isWindows) => isWindows ? "psmux" : "tmux";

    public static LaunchPlan Build(LaunchInputs inputs)
    {
        var backend = BackendFor(inputs.IsWindows);

        List<string> claude = ["claude"];
        if (inputs.HasTeamLeaderAgent)
            claude.AddRange(["--agent", "team-leader"]);
        claude.AddRange(["--teammate-mode", "tmux", "--name", inputs.Session,
            "--append-system-prompt", PeerRule.Marker, .. inputs.Forwarded]);

        // psmux joins arguments without quoting, so a path with spaces would split. The session
        // inherits the client's working directory there instead.
        List<string> newSession = ["new-session"];
        if (inputs.InsidePane)
            newSession.Add("-d");
        newSession.AddRange(["-s", inputs.Session]);
        if (!inputs.IsWindows)
            newSession.AddRange(["-c", inputs.WorkingDirectory]);
        newSession.AddRange(["-e", $"{TeamsVariable}=1", .. claude]);

        List<Command> commands = [new(backend, newSession)];
        if (inputs.InsidePane)
            commands.Add(new(backend, ["switch-client", "-t", "=" + inputs.Session]));
        return new LaunchPlan(backend, commands);
    }
}
