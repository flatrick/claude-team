namespace ClaudeTeam;

public static class Help
{
    public static string Text(string backend, string session) => $"""
        Starts a Claude team leader in a {backend} session named after the current repository.

        Usage:
          claude-team [--session <name>] [--dry-run] [-- <claude arguments>]
          claude-team --install-peer-rule <repo|global|both> [--force]
          claude-team --install-leader-agent <repo|global|both> [--force]

        Options:
          --session <name>              Session name. Default: the origin repository name,
                                        "<repo>+<worktree>" in a linked worktree,
                                        the main checkout folder name without origin,
                                        and the current folder name outside git.
          --dry-run                     Print the session name, the variable, and the commands. Start nothing.
                                        With an install option, print what would be written and write nothing.
          --install-peer-rule <target>  Write {PeerRule.FileName} to .claude/rules/ (repo),
                                        ~/.claude/rules/ (global), or both. Then exit.
          --install-leader-agent <target>
                                        Write {LeaderAgent.FileName} to .claude/agents/ (repo),
                                        ~/.claude/agents/ (global), or both. Also write the skills
                                        {string.Join(", ", TeamSkills.Names)} to
                                        .claude/skills/ or ~/.claude/skills/. Then exit.
                                        Both install options may be given in one run.
          --force                       Replace an existing rule or agent file.
          -h, --help                    Show this help.
          --version                     Print the version and the short commit. Then exit.
          --                            Forward the remaining arguments to claude.

        If the session exists, claude-team stops and starts nothing.
        Inside a {backend} pane, it creates the session detached and switches to it.

        The session for this folder is named: {session}
        """;
}
