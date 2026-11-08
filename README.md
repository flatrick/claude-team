# claude-team

`claude-team` is a launcher. It starts a native Claude agent-team leader in a tmux session. On Windows it uses [psmux](https://github.com/psmux/psmux) with PowerShell 7 and its `tmux` compatibility executable on `PATH`.

## Install

Install from a checkout with `scripts/install-tool.cs`. The script packs the code of the checkout it lives in and installs it as a user-global .NET tool.

```bash
dotnet run --file scripts/install-tool.cs -- install
dotnet run --file scripts/install-tool.cs -- reinstall
dotnet run --file scripts/install-tool.cs -- uninstall
dotnet run --file scripts/install-tool.cs
```

The last command, with no argument, runs `status`. It prints the installed version, the branch of the checkout, and whether `~/.dotnet/tools` is on `PATH`.
A running leader keeps its old build until you restart it. The commands are in [`openspec/specs/tooling/install-tool/`](openspec/specs/tooling/install-tool/).

## Run and attach

Run the command in the folder where you want to work: the main checkout or a linked worktree.

```text
claude-team
```

The launcher runs `claude --teammate-mode tmux --name <name>` in a multiplexer session with the same name. It adds `--agent team-leader` when a `team-leader` agent exists in `.claude/agents/` or `~/.claude/agents/`. Everything after `--` goes to `claude`.

The default name comes from the origin repository name, and is `<repo>+<worktree>` in a linked worktree. Without an origin it is the folder name of the main checkout. Outside a git repository it is the current folder name. Use `--session <name>` to choose another name. `claude-team --help` shows the rules and the options.

If a session with that name exists, the launcher starts nothing and prints how to start under another name, attach, or close the session. To attach, run `tmux attach -t <name>`. On Windows, use `psmux attach -t <name>`.
Inside a multiplexer pane, the launcher creates the session detached and switches to it.

Use `claude-team --dry-run` to print the session name, the variable, and the commands without starting anything.

The launcher sets `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1` for Claude. Claude starts no teammate without it. NOT RE-DERIVED (second-hand — Claude Code agent-teams docs, relayed by a docs agent, not us)
For a new session the launcher also passes the variable with `new-session -e`, because a running tmux server does not take it from the client (`src/ClaudeTeam/Program.cs`, comment before `Processes.Run`). The psmux `new-session -e` flag is NOT RE-DERIVED (psmux docs only; not run on Windows; needs a human).
An existing session keeps its old environment. Run `tmux kill-session -t <name>` (on Windows, `psmux kill-session -t <name>`) before you run the launcher again to apply the variable.

Project `.claude/settings.json` sets `teammateMode` to `tmux`. Claude's [settings reference](https://code.claude.com/docs/en/settings-reference#teammatemode) permits this key in any settings file.

The launcher behavior is in [`openspec/specs/launcher/`](openspec/specs/launcher/).

## Starter files

The launcher can install a starter `team-leader` agent and the peer-leader rule, into the repository, the user profile, or both. It warns at launch when either is missing. Run `claude-team --help` for the options. The behavior is in [`openspec/specs/launcher/starter-files/install/spec.md`](openspec/specs/launcher/starter-files/install/spec.md).

## Several leaders and teammates

The peer-leader rule is [`.claude/rules/claude-team-peer-leaders.md`](.claude/rules/claude-team-peer-leaders.md). The leader and the predefined teammate roles are agent definitions in [`.claude/agents/`](.claude/agents/). Ask the leader for a teammate by agent type. The [`start-teammate`](.claude/skills/start-teammate/SKILL.md) and [`team-protocol`](.claude/skills/team-protocol/SKILL.md) skills define the procedure.

## Where things live

- [`AGENTS.md`](AGENTS.md): repository rules for agents.
- [`openspec/specs/`](openspec/specs/): current behavior of the tool.
- [`FRICTION.md`](FRICTION.md): friction log.
- [`GIT-IT-RIGHT.md`](GIT-IT-RIGHT.md): commit message checking with gir.
