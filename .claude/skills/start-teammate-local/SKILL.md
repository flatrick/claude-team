---
name: start-teammate-local
description: Repository extension of the start-teammate skill. The skill invokes it as its first step. Holds the spawn log, the dotnet-workflow load check, the worktree location, and the NOT RE-DERIVED labels of this repository.
---

# Start a teammate: repository additions

The `start-teammate` skill invokes this skill first. It extends that skill with the conventions of this repository.

## Worktrees

When a teammate needs an isolated checkout, assign a worktree under `.worktrees/` explicitly.

## Observation labels

The rules of this repository need each unverified observation to carry a label.

- The `printenv` command on Windows is NOT RE-DERIVED (not run on Windows; needs a human).
- The cases in which Claude starts no teammate (`-p`, a variable that is not `1`, `isolation`, and a fork) are NOT RE-DERIVED (second-hand — Claude Code agent-teams docs, relayed by a docs agent, not us).

## Spawn log

When a check of "Start one agent at a time" fails, append a row to `.scratch/agent-spawn-log.md` as part of the evidence. If the file is missing, create it with a header row.
Log each start in `.scratch/agent-spawn-log.md` with these columns: name, type, time, config `tmuxPaneId`, real pane id, result of checks 1 to 3, and the confirmation outcome.
The confirmation outcome is one of: passed first time, passed after one reminder, or escalated.

## Confirm the dotnet-workflow load

These presets build or test: `test-runner`, `implementer-standard`, `implementer-hard`, `troubleshooter-standard`, `troubleshooter-hard`, and `troubleshooter-extreme`.
Their first message must also contain `dotnet-workflow loaded`, in any letter case and at any position.
If it does not, send the teammate one message: "Invoke the `dotnet-workflow` skill now and confirm with that line."
If the next message of the teammate still lacks the line, tell the operator. Wait for the decision of the operator.
A missing line is not a launch failure. The "If a check fails, stop" handling does not apply to it.
Do not check this line for the other presets.
