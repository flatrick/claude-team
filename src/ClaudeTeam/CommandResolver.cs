namespace ClaudeTeam;

public enum CommandKind { Help, Version, Install, Update, Launch }

public static class CommandResolver
{
    // Help wins over version, and version wins over an install or update option. Both run before any launch.
    public static CommandKind Resolve(Options options)
    {
        if (options.Help)
            return CommandKind.Help;
        if (options.Version)
            return CommandKind.Version;
        if (options.InstallPeerRule is not null || options.InstallLeaderAgent is not null)
            return CommandKind.Install;
        if (options.Update is not null)
            return CommandKind.Update;
        return CommandKind.Launch;
    }
}
