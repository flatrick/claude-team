# Idle notices from a teammate arrive after a newer leader message and look like a refusal

- Old ID: F-009
- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run. Teammate `versioning-implementer`. Team `session-b1124c40`.
- Observation: An idle notice from the teammate reached the leader after the leader had sent a newer message.
  The notice read as if the teammate had ignored the message.
  This happened two times in the run: at the pack approval and at the stop after task 1.3.
  The notices were stamped 19:22:59 and 19:23:34 UTC.
  When the leader checked, the inbox file `~/.claude/teams/session-b1124c40/inboxes/versioning-implementer.json` was already empty.
  Re-derive the inbox state: `cat ~/.claude/teams/session-b1124c40/inboxes/versioning-implementer.json` (it holds `[]` now; the state at that time is gone).
- Evidence: the idle notices in the leader session. Source: `/home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/friction-pending.md`, item 2.
  NOT RE-DERIVED (conditions gone — the timestamps exist only in the idle notices of the leader session).
- Cause: not verified.
  Hypothesis: the teammate went idle after its previous turn, and the harness delivered the notice late.
  The empty inbox shows that the teammate read the newer message.
- Third case: the `shutdown_approved` message of `versioning-implementer` is stamped 20:00:40 UTC.
  It reached the leader after the leader had confirmed the shutdown by config and pane checks, and after the leader had started `friction-scribe`.
  The `teammate_terminated` system notice came in the same delivery.
  NOT RE-DERIVED (conditions gone — the stamp exists only in the leader session).
- Effect: The leader can judge a teammate wrongly. It can send a message again or stop a running teammate.
  F-004 is a related case: a send result does not show a read, and an idle notice does not show a refusal.
- Status: open.
