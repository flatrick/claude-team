# Proposal

## Why

`claude-team` ships a team-leader agent that tells the leader how to start and end teammates. This repository uses a different leader and the skills `team-protocol`, `start-teammate`, and `end-teammate`.
The users of the tool do not get the files that this repository uses and tests. Improvements in one copy do not reach the other copy.

## What Changes

- The tool embeds and installs the skills `team-protocol`, `start-teammate`, and `end-teammate` in addition to the team-leader agent and the peer-leader rule.
  The tool ships no other agent definition. It ships no worker agent type.
- The shipped files are the single source.
  This repository keeps byte-identical copies of the shipped agent and skills in `.claude/agents/` and `.claude/skills/`. A test fails when a copy differs from its shipped source, as the test for the peer rule does.
  The operator decided this. Reason: improvements cannot drift apart, and the repository always tests what users get.
- Each shipped file has a local extension point: one skill named `<name>-local` for each shipped file (`team-leader-local`, `team-protocol-local`, `start-teammate-local`, `end-teammate-local`).
  The first line in the body of each shipped file is: "Invoke the skill `<own name>-local`. If the tool says `Unknown skill`, continue."
  A repository creates only the `-local` skills that it needs. The tool never ships or installs a `-local` skill.
  The operator decided this. Reasons: each addition loads with its base text, for the same audience and at the same trigger (AGENTS.md rule 3). A skill call works even before the skill listing of a teammate arrives.
- A teammate that loaded `team-protocol-local` starts its first message with `team-protocol loaded +local`. The shipped `start-teammate` tells the leader to require `+local` when `team-protocol-local` exists.
- The shipped leader has the front matter fields `name`, `description`, a neutral `initialPrompt` that invokes `team-protocol`, and `effort: low`. It sets no `model:` and no `memory:`.
  The operator decided this. Reason, in the words of the operator: "the users can change the effort-level if their specific project requires it". The model and the memory are choices of the user.
- The shipped `start-teammate` and `end-teammate` give a psmux form next to each `tmux` command, and a PowerShell form where they need bash or `printenv`. Each psmux form says `not tested on Windows` until a human runs it on Windows.
  The skills say that psmux can differ from tmux and give no error, so the leader judges a start or a shutdown only by the verification checks.
  The operator decided this. Reason: psmux differs from tmux in small ways without an error. For example, a call from the Claude harness that creates several panes under tmux creates only one pane under psmux and reports no error.
- The shipped `team-protocol` takes the report path, the subagent model, and the maximum subagent count only from the assignment. No count means no subagents. No model means the default model of the session. "Preset" becomes "agent type".
  The operator decided this. Reason: the shipped text must not impose the conventions of this repository on other users.
- The shipped text contains no convention of this repository. The text of this repository moves into its `-local` skills: the `dotnet-workflow` gate and load check, the agent types and cost tiers, `friction-scribe`, `decisions.md`, the `.scratch/` report paths, the spawn log, and the `NOT RE-DERIVED` labels.
- `--install-leader-agent <target>` installs the agent and the three skills together, to the same target. `--install-peer-rule` does not change. There is no new option.
  With the target `both`, the tool prints one note: for a name in both places, Claude Code uses the user-profile skill and the repository agent.
  At launch, the tool warns when a shipped skill is in neither location, and when a user-profile copy differs from the repository copy and so hides it.
  The operator decided this. Reason: the agent calls the skills in its first lines, so the agent and the skills are one unit. The shadow warning comes at launch, when it matters, and needs no extra option.
- The help text and the README describe the shipped skills and the `-local` extension point.

## Capabilities

### New Capabilities

- `launcher/starter-files/content`: the content of the shipped starter files: the team-leader agent, the three skills, the `-local` extension point, the load confirmation, and the neutral text.

### Modified Capabilities

- `launcher/starter-files/install`: the starter files include the three skills; the repository copies of the agent and the skills must match their shipped sources; the agent content requirement moves to `launcher/starter-files/content`.
- `launcher/command-line`: the help text for `--install-leader-agent` names the team skills. The accepted options do not change.
- `launcher/session/start`: the launch warns about a missing shipped skill and about a user-profile skill that hides the repository copy.

## Out of scope

- Worker agent types (`implementer-*`, `requirements-*`, `test-runner`, `troubleshooter-*`, `friction-scribe`). They stay repository-only and are not shipped.
- `dotnet-workflow` and every other repository skill. They are not shipped.
- An installer for `-local` skills, and any template for them.
- A merge of user changes into installed files. `--force` replaces a file; without it, the tool keeps the file.
- Changes to `scripts/install-tool.cs` and its specs.
- Any change to how Claude Code resolves skills, agents, or rules.

## Impact

- Code: `src/ClaudeTeam/ClaudeTeam.csproj` (embedded resources), `Installer.cs`, `LeaderAgent.cs`, a new type for the shipped skills, `Options.cs`, `Program.cs`, `Help.cs`.
- Shipped text: `src/ClaudeTeam/team-leader.md` and new sources `src/ClaudeTeam/skills/<name>/SKILL.md`.
- Repository files: `.claude/agents/team-leader.md` and `.claude/skills/{team-protocol,start-teammate,end-teammate}/SKILL.md` become copies of the shipped sources. New `.claude/skills/{team-leader-local,team-protocol-local,start-teammate-local}/SKILL.md` hold the text of this repository.
- Tests: `tests/ClaudeTeam.Tests/` gains copy tests and content tests for the agent and the skills, and install and warning tests for the skills.
- Writes outside the repository: an install with the target `global` or `both` writes `~/.claude/skills/<name>/SKILL.md`, in the same way as the agent and the rule today.
- Docs: `README.md`.
