# claude-team

`claude-team` is a launcher. It starts a native Claude agent-team leader in a tmux session. On Windows, it uses [psmux](https://github.com/psmux/psmux) with PowerShell 7. It uses the `tmux` compatibility executable of psmux on `PATH`.

## Install

Install from a checkout with `scripts/install-tool.cs`. The script packs the code of its own checkout. It installs the code as a user-global .NET tool.

```bash
dotnet run --file scripts/install-tool.cs -- install
dotnet run --file scripts/install-tool.cs -- reinstall
dotnet run --file scripts/install-tool.cs -- uninstall
dotnet run --file scripts/install-tool.cs
```

The last command has no argument and runs `status`. It prints the installed version and the branch of the checkout. It also prints whether `~/.dotnet/tools` is on `PATH`.
A running leader keeps its old build until you restart it. The `status` output also names the commit of the installed build. The commands are in [`openspec/specs/tooling/install-tool/`](openspec/specs/tooling/install-tool/).

Run `claude-team --version` to print the version and the short commit of a build, for example `claude-team 0.1.1-alpha.0.3+bf37b92`. MinVer takes the version from the git tags with the prefix `v`. The behavior is in [`openspec/specs/tooling/release/build-version/`](openspec/specs/tooling/release/build-version/).

## Release

A release is a git tag `vX.Y.Z` on a commit of `origin/main`, and a `CHANGELOG.md` entry. The release commands run locally. Install `git-cliff` first. `scripts/release.cs` never pushes.

1. Work on a branch in a worktree. Use Conventional Commits. Clean up the commits before the pull request.
2. When the branch is ready, run `dotnet run --file scripts/release.cs -- prepare` on the branch. The script asks `git-cliff` for the next version and commits `CHANGELOG.md` as `chore(release): vX.Y.Z`. While the major version is 0, every release is a minor bump. To choose a version, add `--set-version X.Y.Z`.
3. Open the pull request and use "Rebase and merge".
4. In the main checkout, run `git pull --ff-only`. Then run `dotnet run --file scripts/release.cs -- tag`. Push the tag with the command that the script prints.

The release flow needs the rule `chore(release)` in `cliff.toml`. `gir init --force` overwrites `cliff.toml` and removes the rule. After you run it, add the rule again.

The behavior is in [`openspec/specs/tooling/release/`](openspec/specs/tooling/release/).

## Run and attach

Run the command in the folder where you want to work: the main checkout or a linked worktree.

```text
claude-team
```

The launcher runs `claude --teammate-mode tmux --name <name>` in a multiplexer session with the same name. It adds `--agent team-leader` when a `team-leader` agent exists in `.claude/agents/` or `~/.claude/agents/`. The launcher passes everything after `--` to `claude`.

The default name comes from the origin repository name. In a linked worktree, the default name is `<repo>+<worktree>`. Without an origin, the default name is the folder name of the main checkout. Outside a git repository, the default name is the current folder name. Use `--session <name>` to choose another name. `claude-team --help` shows the rules and the options.

If a session with that name exists, the launcher starts nothing. It prints how to start under another name, how to attach, and how to close the session. To attach, run `tmux attach -t <name>`. On Windows, use `psmux attach -t <name>`.
Inside a multiplexer pane, the launcher creates the session detached. Then it switches to the session.

Use `claude-team --dry-run` to print the session name, the variable, and the commands without starting anything.

The launcher sets `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1` for Claude. Without it, Claude starts no teammate. NOT RE-DERIVED (second-hand — Claude Code agent-teams docs, relayed by a docs agent, not us)
For a new session, the launcher also passes the variable with `new-session -e`. A running tmux server does not take the variable from the client (`src/ClaudeTeam/Program.cs`, comment before `Processes.Run`). The psmux `new-session -e` flag is NOT RE-DERIVED (psmux docs only; not run on Windows; needs a human).
An existing session keeps its old environment. To apply the variable, run `tmux kill-session -t <name>` (on Windows, `psmux kill-session -t <name>`). Then run the launcher again.

Project `.claude/settings.json` sets `teammateMode` to `tmux`. The [settings reference](https://code.claude.com/docs/en/settings-reference#teammatemode) of Claude permits this key in any settings file.

The launcher behavior is in [`openspec/specs/launcher/`](openspec/specs/launcher/).

## Starter files

The launcher can install a starter `team-leader` agent and the peer-leader rule. It installs them into the repository, the user profile, or both. At launch, it warns when one of them is missing. Run `claude-team --help` for the options. The behavior is in [`openspec/specs/launcher/starter-files/install/spec.md`](openspec/specs/launcher/starter-files/install/spec.md).

## Several leaders and teammates

The peer-leader rule is [`.claude/rules/claude-team-peer-leaders.md`](.claude/rules/claude-team-peer-leaders.md). The leader and the predefined teammate roles are agent definitions in [`.claude/agents/`](.claude/agents/). Ask the leader for a teammate by agent type. The [`start-teammate`](.claude/skills/start-teammate/SKILL.md) and [`team-protocol`](.claude/skills/team-protocol/SKILL.md) skills define the procedure.

## Where things live

- [`AGENTS.md`](AGENTS.md): repository rules for agents.
- [`openspec/specs/`](openspec/specs/): current behavior of the tool.
- [`friction/README.md`](friction/README.md): friction log, one file for each entry.
- [`GIT-IT-RIGHT.md`](GIT-IT-RIGHT.md): commit message checking with gir.
