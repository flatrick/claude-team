# The implementer reported a write outside the workspace

- Old ID: F-011
- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run. Teammate `versioning-implementer`. Rule: `.claude/rules/workspace-boundary.md`.
- Observation: The implementer said that it wrote a scratch file `/tmp/x` and deleted it.
  `/tmp` is outside the workspace, and the boundary rule requires permission first.
  The source says the message went to the leader during the versioning landing.
- Evidence: `/home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/friction-pending.md`, item 4.
  The leader confirms the source. `versioning-implementer` sent this message to the leader in its "STOP before push" message, about 19:57 UTC on 2026-11-08:
  "Also: I wrote a scratch file in /tmp once (`/tmp/x`, a comparison helper) and deleted it. That was outside the workspace boundary. It had no effect."
  Its idle notice of 19:57:51 UTC repeats it: "Boundary slip: I wrote one scratch file to `/tmp/x` during the spec comparison and deleted it."
  NOT RE-DERIVED (second-hand — `versioning-implementer` reported it to the leader, not to the scribe; the file is gone).
  Separate observation: the written report does not list the `/tmp/x` write under "Writes outside the worktree", so the report is incomplete.
  Re-derive: `rg -n 'Writes outside' /home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/implementer-report.md`.
  The written report names a different outside write: `rg -n 'Restore wrote' /home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/implementer-report.md` gives finding 7, the MinVer package in `~/.nuget/packages`.
  The scribe searched the scratch folder and the versioning ledger: `rg -n '/tmp/x' /home/flatrick/src/github/flatrick/claude-team/.scratch /home/flatrick/src/github/flatrick/claude-team/.worktrees/versioning/.scratch/agent-ledgers`.
  At the time of the first batch it matched only `friction-pending.md`.
- Cause: not verified.
  The report says "Writes outside the worktree: none except finding 7", so the `/tmp/x` write is missing from the written report.
- Effect: The workspace boundary has no hook, so only the teammate can stop a write outside the workspace.
  The written report and the oral report disagree.
- Status: open.
