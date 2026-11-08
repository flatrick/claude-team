---
name: team-protocol
description: Team protocol for the leader and every teammate. Invoke before anything else at startup, before you read the assignment, send a message, or answer a shutdown request.
---

# Team protocol

This skill applies to the leader and to every teammate and subagent that sends messages.

## Load the message schema first

Your first action is ToolSearch with the query `select:SendMessage`.
Do it before you read anything else.
Then every message and every `shutdown_response` uses the correct schema.

## Confirm that you loaded this skill

A teammate starts its first message to the team lead with the exact line `team-protocol loaded`.
Then it announces its native teammate name and preset.

## The assignment

Each assignment names:

- the absolute worktree path,
- the base commit,
- the source files,
- the task,
- the done criteria,
- the approved scope,
- the absolute path of the report,
- the permitted subagent model and the maximum subagent count.

Begin the assignment you were given.
Ask the team lead or the operator only when you received no assignment.
Use native tasks and messages to coordinate.
A message is not an acknowledgment. Wait for a reply before you assume that conflicting work has stopped.
Keep the configuration of your preset. Escalate work beyond its scope to the leader or the operator.

## Subagents

A worker uses Sonnet for subagents, unless the assignment names Opus.
A worker creates at most the number of subagents that the assignment names.
If the assignment names no number, the worker creates none.
Short-lived subagents follow the same ownership rules as the worker.

## Conflicting activity

Obtain approval from the team lead before you start editing, building, or testing that conflicts with another assignment.
Report when you acquire exclusive access and when you release it.
Release it when the assignment ends.
If the leader exits, finish the current assignment and ask the operator what to do next.

## Report

A worker does not rely on memory of an earlier task.
Write the final report to the main checkout's `.scratch/<task>/` folder. The assignment names that path.
The final message of the worker is the report to the leader.
The worker does not send the same report in a separate message.
A worker sends a direct message at once for a blocker, a denied action, or a question.
Each final report ends with `Safe to clear: yes` or `Safe to clear: no` and the reason.

## After a context clear

Read this skill again.
Recover your current assignment from native tasks and messages.
Ask the leader when the assignment is unclear.

## Answer a shutdown request

A teammate receives the request with a `request_id`.
Call `SendMessage` with `to` set to the team lead name from the request, normally `team-lead`.
Set `message` to a JSON OBJECT, not a string: `{"type": "shutdown_response", "request_id": "<id from the request>", "approve": true}`.
Copy the `request_id` exactly.
Approval ends the teammate process.
A string that holds this JSON is rejected with "message text must not be a teammate protocol frame (permission/mode/plan/shutdown JSON) — to respond to a plan or shutdown request, use the structured object form".
If the first response is rejected, read the error and pass the object form. Do not repeat the string form.
