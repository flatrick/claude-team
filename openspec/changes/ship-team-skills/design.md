# Design

## Context

See proposal.md ("Why") for the motivation.

Current state at base commit 2ad9741:

- `src/ClaudeTeam/ClaudeTeam.csproj` (lines 21-24) embeds two resources: `claude-team-peer-leaders.md` and `team-leader.md`.
- `src/ClaudeTeam/Installer.cs` (`Install`, lines 17-29) writes one file to `<root>/.claude/<subfolder>/<file>`. For the target `both`, it writes the repository file first. `Run` (lines 31-48) installs the rule and then the agent, and makes one output line for each file.
- `src/ClaudeTeam/Program.cs` (lines 46-53) warns at launch when the agent or the rule is missing. The project root for the agent is the git top-level folder, or the current folder outside git.
- `tests/ClaudeTeam.Tests/PeerRuleCopyTests.cs` compares the repository rule with the embedded rule.
- The shipped `src/ClaudeTeam/team-leader.md` and the repository `.claude/agents/team-leader.md` are different files. The repository skills `team-protocol`, `start-teammate`, and `end-teammate` are not shipped.

Name precedence in Claude Code, from the probes in the ledger of this change. NOT RE-DERIVED (second-hand: the team lead's probe teammates observed it, not me):

- Skills: a skill in `~/.claude/skills/` hides a repository skill of the same name.
- Agents: a repository agent hides a user profile agent of the same name.
- Rules: both copies load.
- A skill call works before the skill listing of a teammate arrives. A missing skill gives `Unknown skill: <name>`.

## Goals / Non-Goals

**Goals:**

- One source for each shipped team file, and a test that keeps the repository copies the same.
- A local extension point that loads at the same moment, and for the same audience, as the text that it extends.
- Shipped text that works for a user who has none of the conventions of this repository.
- Text in the skills that is usable on Windows with psmux, with each psmux form marked as untested.

**Non-Goals:**

- A launch warning for an installed file that differs from the shipped text, or for two copies that differ. Users can adapt their copies, so a difference is not an error.
- An install record at the target.
- A template or a generator for `-local` skills.

## Decisions

### 1. The shipped files are the single source

The operator decided this. Reason: improvements cannot drift apart, and the repository always tests what users get.

- Put the skill sources in `src/ClaudeTeam/skills/<skill name>/SKILL.md`. Embed each one with the logical name `skills/<skill name>/SKILL.md`.
- Add one type, for example `TeamSkills`, that holds the skill names in install order (`team-protocol`, `start-teammate`, `end-teammate`). It reads each text with `Installer.ReadEmbedded`.
- Replace the content of `.claude/agents/team-leader.md` and of the three repository skills with exact copies of the shipped sources.
- Generalize `PeerRuleCopyTests` into one copy test with a case for each shipped file: the rule, the agent, and each skill. The test compares the text after line-ending normalization, as the current test does.

Alternative: embed the repository files directly through a project link, with no copy. Rejected: the operator chose copies with a test, as for the peer rule. A link also puts a dependency from `src/` on `.claude/`.

### 2. Local extension point: one `-local` skill for each shipped file

The operator decided this. Reasons: each addition loads with its base text, for the same audience and at the same trigger (AGENTS.md rule 3). A skill call works before the skill listing of a teammate arrives.

- The first line of each shipped body is: "Invoke the skill `<own name>-local`. If the tool says `Unknown skill`, continue."
- The tool never ships or installs a `-local` skill. A user profile skill hides a repository skill of the same name, so a user profile `-local` skill would replace the extension of every repository. The shipped text says that a `-local` skill lives in the repository.
- Load confirmations:
  - A teammate writes `team-protocol loaded`, or `team-protocol loaded +local` when it loaded `team-protocol-local`.
  - The shipped `start-teammate` makes the leader require `+local` when the leader itself loaded `team-protocol-local`. The leader knows this from its own startup, so the check does not live in the file that it checks.
  - The leader starts its first message to the operator with `team-leader loaded` or `team-leader loaded +local`. The operator is the only party that reads the load of the leader.
- This repository creates `team-leader-local`, `team-protocol-local`, and `start-teammate-local`. It creates no `end-teammate-local`: I read all 19 lines of `.claude/skills/end-teammate/SKILL.md`, and none holds a convention of this repository.

Alternatives considered (in the earlier review of this change):

- One `team-local` skill for all roles. Rejected: every role loads text for other roles (AGENTS.md rule 3).
- One local skill for the leader and one for teammates. Rejected: the start and end text loads long before its trigger.
- A rule file such as `.claude/rules/team-local.md`. Rejected as the main mechanism: every role gets all text. It stays the fallback if agents skip the first line. A rule file never hides another rule file.

### 3. Install surface, update, and launch warning

The operator chose these points:

- `--install-leader-agent <target>` installs the agent and then the three skills, to the same target. Reason: the agent calls the skills in its first lines, so the agent and the skills are one unit.
- A new option `--update <target>` adds each missing shipped file and replaces each shipped file that differs from the text in the tool. With `--dry-run`, it shows the changes and writes nothing. Reason, in the words of the operator: "I don't want it to warn, as the user's may want to adjust these files to their own needs. I am far more inclined to have --update with a --dry-run to see what would change".
- The update also adds missing files. Reason: a file that a later tool version adds, for example a new skill, must arrive with the next update. The shipped files are one unit, because the leader invokes the skills. Accepted costs: a file that the user deleted on purpose comes back, and the dry run shows this. An update overwrites edits in the files that it replaces, as `--force` does.
- The only launch warning for the skills is for a skill that is in neither location. Reason: the leader then fails at its first step. A copy that differs is not an error, because users can adapt their copies.
- The dry run labels each changed file `older shipped version` or `edited` from an embedded hash list (decision 3a). The tool writes no extra file at the target. Reasons: no new state at the target, and the labels work at once for installs made before this change.

Implementation:

- Each skill is one more `Installer.Install` call with the subfolder `skills/<skill name>` and the file `SKILL.md`. Each file follows the current rules for the result line, `--force`, `--dry-run`, and the outside-git error.
- With the target `both`, for `--install-leader-agent` and `--update`, the output ends with one note. Proposed text: `Note: for a name in both places, Claude Code uses the skill in the user profile and the agent in the repository. Keep both copies the same.` The exit code does not change.
- `Options.cs` parses `--update <target>` with `TryParseInstallTarget`. It rejects `--update` together with an install option. `--force` alone with `--update` stays rejected by the current check, because an update replaces changed files without it. `CommandResolver` gets a new kind `Update` after `Install`. Help and version still win.
- A new `Updater` (or a method on `Installer`) handles each updated file in install order (the rule, the agent, then the skills), for each root of the target. It compares texts after `ReplaceLineEndings("\n")`.
- The line diff is a small longest-common-subsequence diff over lines, in the unified format with three lines of context. The installed files are small (a few hundred lines at most), so the quadratic table is cheap. The tool does not call `git diff`, because git can be absent outside a repository.
- The missing-skill warning is computed by a testable function and printed from `Program.cs` after the peer-rule check, before the dry run. The project root is the same as for the agent (`git?.Toplevel ?? workingDirectory`). Proposed text: `Warning: the team skills <names> are not in .claude/skills/ or ~/.claude/skills/. Install them with: claude-team --install-leader-agent <repo|global|both>`. `<names>` is a comma-separated list in install order.
- `Help.cs` names the skills in the text for `--install-leader-agent`, and adds `--update <target>`.

Alternatives that the operator rejected: a separate `--install-team-skills` option (a user can install an agent with no skills); one `--install` option for all files (breaks current command lines); launch warnings for a copy that differs from the shipped text or from the other copy (users can adapt their copies); an interactive `update` mode (first prompt in the tool); an install record at the target (new state, and no help for older installs).

### 3a. Shipped-version hashes

- Keep the list in `src/ClaudeTeam/shipped-hashes.txt` and embed it. Each line is `<published path> <sha256 hex>`, for example `skills/team-protocol/SKILL.md 3f…`. The path is the path under `.claude/`. The hash is SHA-256 over the UTF-8 text with LF line endings.
- The list holds every shipped version, the current one included. A test fails when the hash of the current embedded text of a file is missing. Because of this, a change to a shipped file adds the new hash in the same change. Each released text is then in the list without a release step, so this change needs no delta to the release specs.
- Seed the list with the hashes of the texts that earlier releases shipped. The update covers the peer-leader rule too. The operator decided this. Reason: one command brings every shipped file up to date, and a user who edited the rule checks the dry run first. Before this change, releases shipped only the rule and `team-leader.md`. Get each earlier text from git with `git show <tag>:src/ClaudeTeam/<file>` for each release tag (`git tag --list 'v*'`). On 2026-11-09 every release tag held one leader text and one rule text. Re-derive with `git log --oneline --follow -- src/ClaudeTeam/<file>` and `for t in $(git tag --list 'v*'); do git show $t:src/ClaudeTeam/<file> | sha256sum; done`. Commits between tags that a user installed from a checkout are not in the list; such a file shows as `edited`.
- Accepted costs, from the operator: the list grows with each change to a shipped file. The tool cannot tell a new file from a deleted file, so it adds each missing file.

### 4. Front matter of the shipped leader

The operator decided: `name`, `description`, a neutral `initialPrompt` that invokes `team-protocol`, and `effort: low`. No `model:` and no `memory:`. Reason, in the words of the operator: "the users can change the effort-level if their specific project requires it".

- Proposed `initialPrompt`: `"Invoke the team-protocol skill before anything else. Then follow your agent definition."`
- The body also tells the leader to invoke `team-protocol` (on the line after the local extension line). If `initialPrompt` does not run, the body still loads the protocol. Whether `initialPrompt` runs for a `--agent` main session: NOT RE-DERIVED (second-hand: the current repository leader uses it, and I did not observe the result).
- How a user changes the effort: edit `effort:` in the installed `team-leader.md`. `--install-leader-agent --force` replaces the file with the shipped text, so the edit is lost on a forced reinstall. A repository agent hides a user profile agent (second-hand, see Context). So a project can keep an edited repository copy, and the user profile keeps the shipped copy.
- This repository does not edit its copy, because the copy test requires the shipped text. Its leader keeps `effort: low`. It loses `model: opus` and `memory: local`. The operator can pass `-- --model opus` at launch.

### 5. psmux and PowerShell forms

The operator decided: name the psmux form next to each `tmux` command, and a PowerShell form where the skill needs bash or `printenv`. Reason from the operator: psmux differs from tmux in small ways and gives no error. For example, tmux can create several panes with one command, but the same call from the Claude harness under psmux creates only one pane and reports no error.

- Commands that get a psmux form: `tmux list-panes -a -F '#{pane_id} #{pane_title}'` and `tmux capture-pane -p -t <pane id>` in `start-teammate`, and the `tmux list-panes` check in `end-teammate`.
- Commands that get a PowerShell form: `printenv CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS` (PowerShell: `$env:CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS`), and the wait loop in `end-teammate`. Proposed PowerShell loop: `$end = (Get-Date).AddSeconds(40); while ((Get-Content "$HOME/.claude/teams/<team>/config.json" -Raw) -match '<name>' -and (Get-Date) -lt $end) { Start-Sleep -Seconds 1 }`.
- Each psmux form carries the words `not tested on Windows`. The shipped text cannot use the `NOT RE-DERIVED` label, because that label is a convention of this repository. A task asks a human to run the checks on Windows. After that run, the words go away for each form that worked.
- Both skills say that psmux can differ from tmux and give no error. The leader judges a start or a shutdown only by the verification checks, never by the exit code of a multiplexer command.
- Support for `list-panes -a -F` and `capture-pane -p` in psmux: NOT RE-DERIVED (not run on Windows; needs a human).

### 6. Neutral team-protocol

The operator decided: the assignment is the only source for the report path, the subagent model, and the maximum subagent count. No count means no subagents. No model means the default model of the session. "Base commit" stays in the assignment list. "Preset" becomes "agent type". Reason: the shipped text must not impose the conventions of this repository. This repository keeps them through `team-protocol-local`, and the `+local` check covers that load.

### 7. Where the text of this repository goes

| Current text | Destination |
|---|---|
| leader: cost tiers, hard and extreme tier rules | `team-leader-local` |
| leader: `dotnet-workflow` gate | `team-leader-local` |
| leader: `friction-scribe` start and `.worktrees/friction-log` | `team-leader-local` |
| leader: `.claude/rules/decisions.md` | `team-leader-local` |
| leader: "Never start an implementer ... as a subagent" | shipped, as "Never start a long-running role as a subagent without the approval of the operator." |
| leader: reuse of teammates, startup check, peer-leaders rule | shipped (no convention of this repository) |
| leader front matter: `model: opus`, `memory: local` | dropped (decision 4) |
| `team-protocol`: Sonnet default for subagents | `team-protocol-local` |
| `team-protocol`: main-checkout `.scratch/<task>/` report path | `team-protocol-local` |
| `team-protocol`: friction goes to the team lead and then to `friction-scribe` | `team-protocol-local` |
| `team-protocol`: "preset" | shipped, as "agent type"; `team-protocol-local` names the presets |
| `start-teammate`: `NOT RE-DERIVED` labels | `start-teammate-local` keeps the labels; the shipped text keeps the documentation link without the label |
| `start-teammate`: `.scratch/agent-spawn-log.md` | `start-teammate-local` |
| `start-teammate`: `dotnet-workflow loaded` check and its preset list | `start-teammate-local` |
| `start-teammate`: "a worktree under `.worktrees/`" | shipped, as "a git worktree"; `start-teammate-local` names `.worktrees/` |

### 8. Platforms and writes outside the repository

- Install and launch code uses `Path.Combine` and the user profile folder from `Environment.SpecialFolder.UserProfile`, as today. The skill paths need no special handling on Windows or on other platforms.
- Writes outside the repository: `--install-leader-agent global` and `both` write `~/.claude/agents/team-leader.md` and `~/.claude/skills/<skill name>/SKILL.md` for each team skill. The tool writes them only when the user gives that option. The launch only reads these files.
- Tests use `Directory.CreateTempSubdirectory` for the repository and the user profile folders, as `LeaderAgentTests` does.

## Risks / Trade-offs

- [An agent skips the first line and does not load its `-local` skill] → The leader requires `+local` from teammates. The operator reads `team-leader loaded +local`. A rule file stays the fallback.
- [A stale user profile skill hides the repository skill, also in this repository] → No warning, by the choice of the operator. `claude-team --update global --dry-run` shows it, with the label `older shipped version` or `edited`. No test can read `~/.claude`, so the copy test does not cover this case.
- [The precedence facts are second-hand] → The design depends on them only for the `Note:` line and for the rule that a `-local` skill lives in the repository. A wrong fact gives a wrong note, not a failed install.
- [A user with an old installed `team-leader.md` runs `--install-leader-agent` without `--force`] → The tool keeps the old agent and writes the new skills. The old agent does not call the skills, so nothing breaks. `--update` replaces the old agent.
- [An update overwrites an edit that the user wanted] → The dry run shows the diff and the label `edited` first. The README tells users to run the dry run before the update.
- [A developer edits a shipped file and forgets the hash] → The hash test fails.
- [psmux behavior is untested] → Each psmux form says `not tested on Windows`. The checks decide a start or a shutdown. A human task runs the checks on Windows.
- [This repository's leader loses `model: opus` and `memory: local`] → The operator passes `-- --model opus` when needed.
- [The help text and the warnings are CLI strings] → They are new behavior in this change, so the specs of this change cover them.

## Migration Plan

1. Write the shipped sources and the tests first. The copy tests fail until the repository copies match.
2. In one commit, replace the repository agent and skills with the shipped text and add the three `-local` skills with the moved text. Then no state on the branch loses a rule of this repository.
3. Users get the skills with `claude-team --install-leader-agent <target>`, or with `claude-team --update <target>`, which also replaces an old agent.
4. Rollback: revert the branch. Installed files stay on the machine of the user until the user removes them.
