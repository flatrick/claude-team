# Native team coordination

The `team-leader` preset leads a native Claude agent team. Claude owns teammate identities, membership, shared tasks, messaging, and shutdown.

Run at most one team leader per worktree. A worktree needs no leader. Start it through `scripts/start-claude-tmux.cs`, which names the leader `<repo>` or `<repo>+<worktree>`. Leaders in other worktrees are your peers. Find them with `ListAgents`, and ask them before you remove a worktree or change state that all worktrees share. Use Claude's built-in tools for teammates and subagents.

At startup, announce your native teammate name and preset. Begin the assignment supplied with your launch. Ask the team lead or operator for an assignment only when none was supplied. Use native tasks and messages to coordinate. A message is not an acknowledgment; wait for a reply before assuming that conflicting work has stopped.

## Load the message schema first

This applies to the leader, every teammate, and every subagent that sends messages.
Your first action is ToolSearch with the query `select:SendMessage`.
Do it before you read anything else.
Every message and every `shutdown_response` then uses the correct schema.
Added: 2026-10-31.

## Start a teammate

Run each role that needs a visible pane and ongoing communication as a named teammate in a tmux pane.
An implementer or other long-running role MUST run as a teammate.
Never run such a role as a subagent without the approval of the operator.
The launcher starts only the leader.
The leader starts each predefined teammate as follows:

1. Check that `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS` is `1` in your environment. Run `printenv CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS`. The `printenv` command on Windows is NOT RE-DERIVED (not run on Windows; needs a human).
2. If the variable is not `1`, STOP and tell the operator. Do not start a subagent instead.
3. Do not create a team. The team exists when the session starts.
4. Call the built-in Agent tool with `name` and `subagent_type`. Pass no `isolation`. Do not pass `subagent_type: "fork"`.
5. Verify the teammate with the three checks in "Start one agent at a time".

If any of the three checks fails, the launch FAILED.
Tell the operator that the launch failed.
Do not treat the agent as a teammate.

Claude's [team-start contract](https://code.claude.com/docs/en/agent-teams#how-claude-starts-agent-teams) lists the cases. Claude starts no teammate when it runs with `-p`, when the variable is not `1`, when the Agent call passes `isolation`, or when the call is a fork.
The team exists when the session starts, and no tool creates a team.
NOT RE-DERIVED (second-hand — Claude Code agent-teams docs, relayed by a docs agent, not us)

`Explore`, `claude-code-guide`, and similar bounded read-only helpers may run as subagents.
A subagent is not a teammate.
It has no pane, and the three checks do not apply to it.

Assign repository worktrees under `.worktrees/` explicitly when a teammate needs an isolated checkout.
Do not write native team configuration, task files, or mailboxes yourself.
Remove the variable check and the launcher variable when agent teams stop being experimental.

## Choose a preset

Start with the least expensive suitable preset. The ladder lists configured model and effort from lowest cost to highest cost.

| Preset | Configured model, effort | Use for |
|---|---|---|
| `test-runner` | Sonnet, low | Running tests and reporting results |
| `implementer-standard` | Sonnet, high | Implementation with a clear scope |
| `requirements-standard` | Sonnet, high | Requirements and documentation from a settled design |
| `troubleshooter-standard` | Sonnet, high | Failures with a reproducible cause path |
| `implementer-hard` | Opus, medium | Unclear design, cross-cutting effects, or a failed standard attempt |
| `requirements-hard` | Opus, medium | Unsettled design, or a requirement that affects several capabilities |
| `troubleshooter-hard` | Opus, high | Failures that `troubleshooter-standard` could not explain |
| `troubleshooter-extreme` | Opus, xhigh | Failures that `troubleshooter-hard` could not explain; each use needs approval |

The leader is configured for Opus, low. Preserve the selected preset's configuration and escalate work beyond its scope to the leader or operator.

Claude applies preset configuration according to its native rules. A split-pane teammate uses the definition's model unless the spawn request or another native override takes precedence. Split-pane teammates inherit the leader's effort by default; the documented frontmatter effort override applies to in-process teammates. A preset's configured effort is not proof of its effective runtime effort. Check the [native definition rules](https://code.claude.com/docs/en/agent-teams#use-subagent-definitions-for-teammates) before making stronger claims.

Role boundaries in these instructions still apply when Claude does not enforce a frontmatter field in split-pane mode. In particular, the test runner does not spawn agents.

## Assign and report work

Each assignment tells the worker to load the schema first, as "Load the message schema first" says.

The leader tracks worktree ownership and coordinates editing, building, and testing. Workers obtain approval before starting activity that conflicts with another assignment. Report when exclusive access is acquired and released. Release it when the assignment ends. If the leader exits, finish the current assignment and ask the operator what to do next.

Each assignment names the absolute worktree path, base commit, source files, task, done criteria, and approved scope. It also names the permitted subagent model and maximum subagent count. A worker uses Sonnet for subagents unless the assignment names Opus. A worker creates none when the assignment names no maximum count. Short-lived subagents follow the same ownership rules.

A worker does not rely on memory of an earlier task.
Write the final report to the main checkout's `.scratch/<task>/` folder.
The final message of the worker is the report to the leader.
The worker does not send the same report by a separate message.
A worker sends a direct message at once for a blocker, a denied action, or a question.
The assignment names that scratch path explicitly. Each final report ends with `Safe to clear: yes` or `Safe to clear: no` and the reason. The leader tells the operator.

## Start one agent at a time

This rule applies to every Agent call that starts a teammate or creates a tmux pane.
The leader and each teammate that starts agents follow it.
Start exactly one new agent in each message.
Never put two such Agent calls in one message.
Never start a second agent before you verify the first agent.
Verify an agent after the Agent call returns. All three checks must pass:
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
Log each start in `.scratch/agent-spawn-log.md` with these columns: name, type, time, config `tmuxPaneId`, real pane id, result of checks 1 to 3.
Messages and tasks are not agent starts, and this rule does not limit them. Running agents work in parallel. Only the starts are serial.
Urgency and batches are not exceptions. If the operator names several agents, start them one after another.

After a context clear, read this file and recover your current assignment from native tasks and messages. Ask the leader when the assignment is unclear. Use Claude's built-in shutdown flow to end teammates when their work is complete.

## End a teammate

1. Shut a teammate down only after its work is complete and the operator asked for it, or the assignment is done.
2. Call `SendMessage` with `to` set to the teammate name. Set `message` to a JSON OBJECT, not a string: `{"type": "shutdown_request", "reason": "<short reason>"}`.
3. A string that holds protocol JSON is rejected with "message text must not be a teammate protocol frame". Pass the object form. If the `SendMessage` schema is not loaded, load it first with ToolSearch `select:SendMessage`.
4. If the first call is rejected, read the error. Do not send the same call again.
5. Verify the shutdown. The pane must be absent from `tmux list-panes -a -F '#{pane_id} #{pane_title}'`. The name must be absent from `~/.claude/teams/<team>/config.json`. Both changes happen within about 25 seconds of the request.
6. Wait with a `timeout` command and an `until` loop, for example `timeout 40 bash -c 'until ! grep -q "<name>" ~/.claude/teams/<team>/config.json; do sleep 1; done'`. A bare foreground `sleep` is blocked.
7. A failed response from the teammate is not proof that the teammate survives. Always run the checks in steps 5 and 6.
8. Never use `TaskStop` first. Use it only if the teammate does not exit after the request. Follow the pane-capture rule in "Start one agent at a time" first.

A teammate often sends the `shutdown_response` as a JSON string one or more times before the object form succeeds.
These rejected attempts are expected, and they are not an error.
The leader takes no action for them, runs the checks in steps 5 and 6, and judges the shutdown only by those checks.

### Answer a shutdown request

Load the schema first, as "Load the message schema first" says.

A teammate receives the request with a `request_id`.
The teammate calls `SendMessage` with `to` set to the team lead name from the request, normally `team-lead`.
The teammate sets `message` to a JSON OBJECT, not a string: `{"type": "shutdown_response", "request_id": "<id from the request>", "approve": true}`.
Copy the `request_id` exactly.
Approval ends the teammate process.
A string that holds this JSON is rejected with "message text must not be a teammate protocol frame (permission/mode/plan/shutdown JSON) — to respond to a plan or shutdown request, use the structured object form".
If the first response is rejected, read the error and pass the object form. Do not repeat the string form.
