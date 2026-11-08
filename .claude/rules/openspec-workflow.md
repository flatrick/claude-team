# OpenSpec workflow

Added: 2026-11-08.

## Rule

Use an OpenSpec change for every change that adds, removes, or alters behavior that a user of `claude-team` or `scripts/install-tool.cs` can observe.
Skip OpenSpec for a fix that restores specified behavior, a refactor, a test-only change, and a documentation-only change.
The specs in `openspec/specs/` describe current behavior.
`openspec/config.yaml` gives the constraints that every artifact follows.

- Name the change after the worktree slug. The change folder, the worktree `.worktrees/<slug>/`, and the ledger `.scratch/agent-ledgers/<slug>/` use the same name.
- Create, edit, and archive the change in its worktree. Commit the artifacts on the same branch as the code. Never write change artifacts in the main checkout.
- A `requirements-*` preset writes the proposal, the delta specs, the design, and the tasks. Use `requirements-hard` when the design is unsettled or the change touches several specs.
- An `implementer-*` preset applies the tasks with `/opsx:apply`. It does not rewrite the proposal, specs, or design. It reports a needed change to the team lead.
- Run `/opsx:verify` before a PR into main. Verify includes the code review: check that every intended change was made and that the change fulfills its proposal.
- Run `openspec validate <change> --strict` before you commit change artifacts. Keep validation free of warnings, as [openspec-validation-warnings.md](openspec-validation-warnings.md) says.
- Archive the change with `/opsx:archive` in the worktree before the branch merges, so the synced specs merge with the code.
- When an `/opsx:*` command says to ask or confirm with the user, the teammate asks the team lead, who asks the operator as [decisions.md](decisions.md) says.
- An artifact states each decision and its reason in full.
- Write the results of `/opsx:verify` and of each review to the ledger, as [agent-findings-ledger.md](agent-findings-ledger.md) says.
- Do not edit `.claude/commands/opsx/`. `openspec update` manages these files. Put repository constraints in this rule or in `openspec/config.yaml`.

## Spec layout

Put each spec in its own folder as `spec.md`.
OpenSpec reads only files named `spec.md`. It skips files with any other name and rejects delta specs with any other name.
A folder that holds `spec.md` is a spec. A folder without one is an area or a sub-area.
Any folder may hold any number of spec folders.

| Scope | Path under `openspec/specs/` | Applies to |
|---|---|---|
| Solution-wide shared spec | `<name>/spec.md` | the whole solution |
| Area-shared spec | `<AREA>/<name>/spec.md` | every spec under `<AREA>/` |
| Leaf spec | `<AREA>/<SUB-AREA>/<name>/spec.md` | one part of a sub-area |

Put a requirement in a shared spec only when it applies to the whole solution or the whole area.
Do not repeat a shared requirement in a leaf spec.
A delta spec in a change uses the same path as the main spec it changes.

## Why

A spec that no longer matches the code misleads the next agent, which trusts the spec over the code.
A change folder outside its worktree splits the change from its code, the same way a split ledger splits findings.

## Enforcement

Behavioral.
`openspec validate --strict` checks the format of specs, not whether they match the code.
`/opsx:verify` and the reviewer check that a behavior change also changed `openspec/`, and that each requirement cites its source file.
