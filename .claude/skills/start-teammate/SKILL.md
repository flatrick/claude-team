---
name: start-teammate
description: Leader only. Invoke just before an Agent call that starts a teammate, and again before each further start. Covers the variable check, the call shape, one start per message, and the verification checks.
---

# Start a teammate

Run each role that needs a visible pane and ongoing communication as a named teammate in a tmux pane.
The launcher starts only the leader. Do not use it or shell commands to launch workers.

## Start

1. Check that `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS` is `1` in your environment. Run `printenv CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS`. The `printenv` command on Windows is NOT RE-DERIVED (not run on Windows; needs a human).
2. If the variable is not `1`, STOP and tell the operator. Do not start a subagent instead.
3. Do not create a team. The team exists when the session starts.
4. Call the built-in Agent tool with `name` and `subagent_type`. Pass no `isolation`. Do not pass `subagent_type: "fork"`.
5. Verify the teammate with the checks below.

If any check fails, the launch FAILED.
Tell the operator that the launch failed.
Do not treat the agent as a teammate.

Claude's [team-start contract](https://code.claude.com/docs/en/agent-teams#how-claude-starts-agent-teams) lists the cases. Claude starts no teammate when it runs with `-p`, when the variable is not `1`, when the Agent call passes `isolation`, or when the call is a fork.
The team exists when the session starts, and no tool creates a team.
NOT RE-DERIVED (second-hand — Claude Code agent-teams docs, relayed by a docs agent, not us)

`Explore`, `claude-code-guide`, and similar bounded read-only helpers may run as subagents.
A subagent is not a teammate.
It has no pane, and the checks do not apply to it.

Assign repository worktrees under `.worktrees/` explicitly when a teammate needs an isolated checkout.
Do not write native team configuration, task files, or mailboxes yourself.
Let Claude manage membership, shared tasks, messages, and shutdown.
Remove the variable check and the launcher variable when agent teams stop being experimental.

## Split-pane definition fields

Claude applies preset configuration according to its native rules.
A split-pane teammate uses the definition's model unless the spawn request or another native override takes precedence.
Split-pane teammates inherit the leader's effort by default; the documented frontmatter effort override applies to in-process teammates.
A preset's configured effort is not proof of its effective runtime effort.
Check the [native definition rules](https://code.claude.com/docs/en/agent-teams#use-subagent-definitions-for-teammates) before you make stronger claims.
Role boundaries in the agent definitions still apply when Claude does not enforce a frontmatter field in split-pane mode.

## Start one agent at a time

This rule applies to every Agent call that starts a teammate or creates a tmux pane.
Start exactly one new agent in each message.
Never put two such Agent calls in one message.
Never start a second agent before you verify the first agent.
Verify an agent after the Agent call returns. Every check must pass:
1. Run `tmux list-panes -a -F '#{pane_id} #{pane_title}'`. The pane id in the `tmuxPaneId` of the member entry of the agent must exist in the output.
2. Read `~/.claude/teams/<team>/config.json`. The member entry of the agent must have a `tmuxPaneId` equal to that pane id. No other member entry may have the same `tmuxPaneId`.
3. The agent must be active. It sent a first message or progress report, or `ListAgents` lists it and its file under `inboxes/` is `[]`.

If tmux is not available in the shell, say so and use checks 2 and 3 only.
If a check fails, stop. Do not start another agent. Do not use `TaskStop` yet.
Capture evidence first: run `tmux capture-pane -p -t <pane id>` and copy the config entry.
Append a row to `.scratch/agent-spawn-log.md`. Create the file with a header row if it is missing.
Tell the operator and wait for the decision of the operator.
Before `TaskStop` on an agent, capture its pane with `tmux capture-pane -p -t <pane id>`.
Check that no other agent has the same pane id. A stop can close the pane of another agent.
Log each start in `.scratch/agent-spawn-log.md` with these columns: name, type, time, config `tmuxPaneId`, real pane id, result of checks 1 to 3 and the confirmation outcome: passed first time, passed after one reminder, or escalated.
Messages and tasks are not agent starts, and this rule does not limit them. Running agents work in parallel. Only the starts are serial.
Urgency and batches are not exceptions. If the operator names several agents, start them one after another.

## Confirm the protocol load

The first message of the teammate passes when it contains `team-protocol loaded`, in any letter case and at any position.
If it does not, send the teammate one message: invoke the `team-protocol` skill now and confirm with that line.
If its next message still lacks the line, tell the operator and wait for the decision of the operator.
A missing line is not a launch failure. The "If a check fails, stop" handling does not apply to it.
