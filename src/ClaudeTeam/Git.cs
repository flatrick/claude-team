namespace ClaudeTeam;

public static class Git
{
    public static GitInfo? Read()
    {
        var (exitCode, output) = Processes.Capture("git", "rev-parse", "--path-format=absolute", "--show-toplevel", "--git-dir", "--git-common-dir");
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (exitCode != 0 || lines.Length != 3)
            return null;

        var origin = Processes.Capture("git", "remote", "get-url", "origin");
        return new GitInfo(lines[0], lines[1], lines[2], origin.ExitCode == 0 && origin.Output.Length > 0 ? origin.Output : null);
    }
}
