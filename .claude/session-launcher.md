# Start a native Claude team leader

Run the claude-team launcher to start a team leader in a tmux session. On Windows the launcher uses [psmux](https://github.com/psmux/psmux) with PowerShell 7 and its `tmux` compatibility executable on `PATH`.

```text
claude-team
```

Run the command in the folder where you want to work: the main checkout or a linked worktree. Use `claude-team --help` to see the options.

The launcher runs `claude --agent team-leader --teammate-mode tmux --name <name>` in a multiplexer session with the same name. The name is the repository folder name in the main checkout, and `<repo>+<worktree>` in a linked worktree. Outside a git repository the name is `claude-session`. Use `--session <name>` to choose another name.

Run the launcher again in the same folder to attach to its existing session. Run the launcher inside a multiplexer pane to start the leader in that pane. To attach from another terminal, run `tmux attach -t <name>`. On Windows, use `psmux attach -t <name>`.

The launcher sets `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1` for Claude. Claude starts no teammate without it. NOT RE-DERIVED (second-hand — Claude Code agent-teams docs, relayed by a docs agent, not us)
For a new session the launcher also passes the variable with `new-session -e`. The psmux `new-session -e` flag is NOT RE-DERIVED (psmux docs only; not run on Windows; needs a human).
An existing session keeps its old environment, and `new-session -A` ignores `-e` when it attaches. Run `tmux kill-session -t <name>` (on Windows, `psmux kill-session -t <name>`) before you run the launcher again to apply the variable.
Use `claude-team --dry-run` to print the command and the variable without starting Claude.
Remove the variable from the launcher when agent teams stop being experimental.

Project `.claude/settings.json` sets `teammateMode` to `tmux`. Claude's [settings reference](https://code.claude.com/docs/en/settings-reference#teammatemode) permits this key in any settings file.

## Install the starter leader and the peer rule

The launcher passes `--agent team-leader` only when a `team-leader` agent exists in `.claude/agents/` or `~/.claude/agents/`. Otherwise it warns, starts Claude without `--agent`, and prints this hint:

```
claude-team --install-leader-agent <repo|global|both>
```

The command writes an embedded, repository-independent starter definition to `.claude/agents/team-leader.md` (repo), `~/.claude/agents/team-leader.md` (global), or both, then exits. It keeps an existing file unless `--force` is given. `--install-peer-rule <repo|global|both>` works the same way for the peer-leader rule in `.claude/rules/`. Give both install options in one run to install both files. `--force` applies to every install option in the run. With `--dry-run`, an install option prints `Would write <path>` or `Would keep the existing <path>` and writes nothing; the exit code matches the real run.

## Several leaders

Run at most one leader per worktree. A worktree needs no leader. The launcher adds a peer rule to each leader. A leader finds its peers with `ListAgents` and contacts them with `SendMessage`. It asks every peer before it removes a worktree, or before it changes state shared by all worktrees. The launcher holds the rule as a constant. Nothing enforces the rule mechanically.

## Ask Claude for teammates

Give the leader a goal. Ask for a predefined role by its agent type, for example:

```text
Spawn a teammate using the implementer-standard agent type for this assignment.
```

Claude uses its built-in Agent tool to launch predefined teammates from `.claude/agents/*.md`. The Agent call passes `name` and `subagent_type`. The [`start-teammate` skill](skills/start-teammate/SKILL.md) defines the start procedure, and the [`team-protocol` skill](skills/team-protocol/SKILL.md) defines the assignment boundaries. The [Claude teammate definition reference](https://code.claude.com/docs/en/agent-teams#use-subagent-definitions-for-teammates) describes which fields split-pane mode applies.
