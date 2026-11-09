---
name: start-teammate
description: Leader only. Invoke just before an Agent call that starts a teammate, and again before each further start. Covers the variable check, the call shape, one start per message, and the verification checks.
---

Invoke the skill `start-teammate-local`. If the tool says `Unknown skill`, continue.

# Start a teammate

Run each role that needs a visible pane and ongoing communication as a named teammate in a multiplexer pane.
The multiplexer is tmux on Linux, macOS, and WSL, and psmux on Windows.
psmux can behave differently from tmux and give no error. Judge a start only by the verification checks below, never by the exit code of a multiplexer command.
The launcher starts only the leader. Do not use the launcher or shell commands to start workers.

## Start

1. Check that `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS` is `1` in your environment.
   - On Linux and macOS, run `printenv CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS`.
   - In PowerShell on Windows, read `$env:CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS` (not tested on Windows).
2. If the variable is not `1`, STOP and tell the operator. Do not start a subagent instead.
3. Do not create a team. The team exists when the session starts.
4. Call the built-in Agent tool with `name` and `subagent_type`. Pass no `isolation`. Do not pass `subagent_type: "fork"`.
5. Verify the teammate with the checks below.

If any check fails, the launch FAILED.
Tell the operator that the launch failed.
Do not treat the agent as a teammate.

The [team-start contract](https://code.claude.com/docs/en/agent-teams#how-claude-starts-agent-teams) of Claude lists the cases. Claude starts no teammate in these cases:

- Claude runs with `-p`.
- The variable is not `1`.
- The Agent call passes `isolation`.
- The call is a fork.

The team exists when the session starts, and no tool creates a team.

`Explore`, `claude-code-guide`, and similar bounded read-only helpers can run as subagents.
A subagent is not a teammate.
It has no pane, and the checks do not apply to it.

When a teammate needs an isolated checkout, assign a git worktree explicitly.
Do not write native team configuration, task files, or mailboxes yourself.
Let Claude manage membership, shared tasks, messages, and shutdown.
When agent teams stop being experimental, remove the variable check and the launcher variable.

## Split-pane definition fields

Claude applies the configuration of an agent definition according to its native rules.
A split-pane teammate uses the model of the definition, unless the spawn request or another native override takes precedence.
Split-pane teammates inherit the effort of the leader by default. The documented frontmatter effort override applies to in-process teammates.
The configured effort of an agent definition is not proof of its effective runtime effort.
Before you make a stronger claim, check the [native definition rules](https://code.claude.com/docs/en/agent-teams#use-subagent-definitions-for-teammates).
The role boundaries in the agent definitions still apply when Claude does not enforce a frontmatter field in split-pane mode.

## Start one agent at a time

This rule applies to every Agent call that starts a teammate or creates a pane.
Start exactly one new agent in each message.
Never put two such Agent calls in one message.
Never start a second agent before you verify the first agent.
After the Agent call returns, verify the agent. Every check must pass:
1. List the panes. The pane id in the `tmuxPaneId` of the member entry of the agent must exist in the output.
   - tmux: `tmux list-panes -a -F '#{pane_id} #{pane_title}'`
   - psmux: `psmux list-panes -a -F '#{pane_id} #{pane_title}'` (not tested on Windows)
2. Read `~/.claude/teams/<team>/config.json`. The member entry of the agent must have a `tmuxPaneId` equal to that pane id. No other member entry can have the same `tmuxPaneId`.
3. The agent must be active. Either it sent a first message or progress report, or `ListAgents` lists it and its file under `inboxes/` is `[]`.

If the multiplexer is not available in the shell, say so and use checks 2 and 3 only.
If a check fails, stop. Do not start another agent. Do not use `TaskStop` yet.
First, capture evidence: capture the pane and copy the config entry.

- tmux: `tmux capture-pane -p -t <pane id>`
- psmux: `psmux capture-pane -p -t <pane id>` (not tested on Windows)

Tell the operator. Wait for the decision of the operator.
Before `TaskStop` on an agent, capture its pane in the same way.
Check that no other agent has the same pane id. A stop can close the pane of another agent.
Messages and tasks are not agent starts, and this rule does not limit them. Running agents work in parallel. Only the starts are serial.
Urgency and batches are not exceptions. If the operator names several agents, start them one after another.

## Confirm the protocol load

The first message of the teammate passes when it contains `team-protocol loaded`, in any letter case and at any position.
If you loaded `team-protocol-local` yourself, the line must be `team-protocol loaded +local`.
If the line is missing, or lacks `+local` when you need it, send the teammate one message: "Invoke the `team-protocol` skill now and confirm with that line."
If the next message of the teammate still lacks the line, tell the operator. Wait for the decision of the operator.
A missing line is not a launch failure. The "If a check fails, stop" handling does not apply to it.
