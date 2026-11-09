---
name: team-leader-local
description: Repository extension of the team-leader agent. The agent invokes it as its first step. Holds the presets, the cost tiers, the build gate, the friction scribe, and the decision files of this repository.
---

# Team leader: repository additions

The `team-leader` agent invokes this skill first. It extends the agent with the conventions of this repository.
When you load this skill, write `team-leader loaded +local` in your first message to the operator.
Your preset is `team-leader`. Announce your name and your preset to the operator.

## Cost tiers

Start with the least expensive suitable preset.
Use a hard tier only after a standard tier failed, or when the task description meets the hard-tier criteria.
The extreme tier needs the approval of the operator for each use.

## Build and test gate

Before you assign or judge a build or test gate, invoke the `dotnet-workflow` skill and follow its gate section.

## Friction scribe

At the first friction point of a session, start a `friction-scribe` teammate. A session with no friction then costs nothing extra.
If the worktree `.worktrees/friction-log` does not exist, create it before the start.
Send the scribe each friction point with its evidence. Keep the scribe until the session ends.
The [`friction-scribe`](../../agents/friction-scribe.md) preset defines the rest.

## Operator decisions

Ask the operator for each decision as `.claude/rules/decisions.md` says.
Do not ask for a decision with long text in chat.
