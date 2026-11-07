namespace ClaudeTeam;

public enum SessionState { Absent, Exists, BackendMissing }

public static class Multiplexer
{
    public static SessionState Check(string backend, string session)
        => Processes.Capture(backend, "has-session", "-t", "=" + session).ExitCode switch
        {
            0 => SessionState.Exists,
            Processes.NotFound => SessionState.BackendMissing,
            _ => SessionState.Absent,
        };

    public static string ExistsMessage(string backend, string session) => $"""
        A {backend} session named '{session}' already exists. Nothing was started.
        Choose one:
          Start with a unique name:  claude-team --session {session}-2
          Attach to it:              {backend} attach -t ={session}
          Close it down:             {backend} kill-session -t ={session}
        """;
}
