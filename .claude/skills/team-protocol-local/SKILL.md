---
name: team-protocol-local
description: Repository extension of the team-protocol skill. The skill invokes it as its first step. Holds the preset names, the Sonnet default for subagents, the report location, and the friction path of this repository.
---

# Team protocol: repository additions

The `team-protocol` skill invokes this skill first. It extends that skill with the conventions of this repository.
When you load this skill, start your first message to the team lead with `team-protocol loaded +local`.

## Presets

The agent type of a teammate is its preset, a definition in `.claude/agents/`.
Announce your native teammate name and your preset in your first message.
Keep the configuration of your preset. Escalate work beyond its scope to the leader or the operator.

## Subagents

A worker uses Sonnet for subagents, unless the assignment names Opus.
This default applies when the assignment names no model.

## Report

Write the final report to the main checkout's `.scratch/<task>/` folder. The assignment names that path.
Report friction to the team lead. The team lead forwards it to the `friction-scribe` teammate.
