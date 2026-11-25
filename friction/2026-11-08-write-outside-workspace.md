# The implementer reported a write outside the workspace

- Old ID: F-011
- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run. Teammate `versioning-implementer`. Rule: `.claude/rules/workspace-boundary.md`.
- Observation: The implementer said that it wrote a scratch file `/tmp/x` and deleted it.
  `/tmp` is outside the workspace, and the boundary rule requires permission first.
  The source says the message went to the leader during the versioning landing.
- Evidence: item 4 of the friction note of the run: "The implementer wrote a scratch file to /tmp (`/tmp/x`) and deleted it, outside the workspace boundary. Source: its message to the lead during the versioning landing."
  The leader confirms the source. `versioning-implementer` sent this message to the leader in its "STOP before push" message, about 19:57 UTC on 2026-11-08:
  "Also: I wrote a scratch file in /tmp once (`/tmp/x`, a comparison helper) and deleted it. That was outside the workspace boundary. It had no effect."
  Its idle notice of 19:57:51 UTC repeats it: "Boundary slip: I wrote one scratch file to `/tmp/x` during the spec comparison and deleted it."
  NOT RE-DERIVED (second-hand — `versioning-implementer` reported it to the leader, not to the scribe; the file is gone).
  Separate observation: the report of `versioning-implementer` said "Writes outside the worktree: none except finding 7." Finding 7 is a different outside write: the restore wrote the MinVer package to `~/.nuget/packages`.
  When the scribe first wrote this entry, a search of the temporary files for `/tmp/x` matched only the friction note of the run.
  On 2026-11-25, the scribe saw two more written statements about the `/tmp` write:
  - A later section of the same report said: "One /tmp write happened and was deleted."
  - The round 6 ledger entry of `versioning-implementer` said: "I wrote one helper file to /tmp during the spec comparison and deleted it. This was outside the workspace boundary."
  - Neither statement names `/tmp/x`. The scribe does not know when `versioning-implementer` wrote them.
  - Thus the claim that the written record omits the `/tmp` write is not correct for these two statements.
  NOT RE-DERIVED (conditions gone — the report and the ledger were temporary files).
  The line "Writes outside the worktree: none except finding 7" does not include the `/tmp/x` write.
- Effect: The workspace boundary has no hook, so only the teammate can stop a write outside the workspace.
  One line of the written report and the oral report disagree.
- Status: open.
