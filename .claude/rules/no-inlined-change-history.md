# No inlined change history in current-truth documents

## Rule

A document that states **current truth** contains only what is true now.
Current-truth documents are rules, skills, agent briefs, the README, and the specs in `openspec/specs/`.
Do not write the history of the document inside it.

Forbidden inside a current-truth document:

- Changelog footers, for example `*Calibrated 2026-07-15. This document previously demanded…*`.
- Diff narration: "this previously said X", "was removed in", "used to require", "no longer applies".
- Dated provenance stamps on ordinary claims, for example "(verified 2026-07-15)".
- Migration or deprecation asides that only explain a past decision.

Put history in the git commit message, the PR text, or a separate dated file under `.scratch/`.

## Why

A reader must separate current rules from historical text. Every stale aside becomes a future contradiction.
An agent that reads a change note takes it as current guidance.
A correction that names the wrong value in a footer puts the wrong value back in the reader's context.

## The keep-or-cut test

Ask: would this sentence be worth writing if the document had always said the right thing?

- Yes: keep it. It is a current fact, for example a warning about a live trap.
- No: cut it. It only makes sense as "we changed this".

State the fact and drop the stamp.
Evidence from elsewhere in the repository is allowed in a "Why" section.
A live warning about a trap is a current fact. Do not remove it.

## Scope

This rule applies to `.claude/rules/`, `.claude/skills/`, `.claude/agents/`, `openspec/specs/`, and `README.md`.
It does not apply to commit messages, PR text, friction logs, or files under `.scratch/`.

## Enforcement

Behavioral.
No pattern can separate history from honest prose.
Check each document you edit, and remove change history that you find.
