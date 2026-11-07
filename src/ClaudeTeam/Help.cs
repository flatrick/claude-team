namespace ClaudeTeam;

public static class Help
{
    public static string Text(string backend, string session) => $"""
        Starts a Claude team leader in a {backend} session named after the current repository.

        Usage:
          claude-team [--session <name>] [--dry-run] [-- <claude arguments>]
          claude-team --install-peer-rule <repo|global|both> [--force]

        Options:
          --session <name>              Session name. Default: the origin repository name,
                                        "<repo>+<worktree>" in a linked worktree,
                                        the main checkout folder name without origin,
                                        and the current folder name outside git.
          --dry-run                     Print the session name, the variable, and the commands. Start nothing.
          --install-peer-rule <target>  Write {PeerRule.FileName} to .claude/rules/ (repo),
                                        ~/.claude/rules/ (global), or both. Then exit.
          --force                       Replace an existing rule file.
          -h, --help                    Show this help.
          --                            Forward the remaining arguments to claude.

        If the session exists, claude-team stops and starts nothing.
        Inside a {backend} pane, it creates the session detached and switches to it.

        The session for this folder is named: {session}
        """;
}
