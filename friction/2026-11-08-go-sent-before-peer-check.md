# The leader sent the go before it read the peer check

- Old ID: F-015
- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: v0.2.0 release. Leader process error. Teammate `release-v020`. Rule: `.claude/rules/claude-team-peer-leaders.md`.
- Observation: The leader sent `ListAgents` and the go for the reinstall to `release-v020` in the same tool batch.
  The go left before the leader read the `ListAgents` result.
  The result showed a new online peer, `death-angel-breezy-squid`.
  The leader sent a hold and asked the peer. The peer was `release-v020` itself (third case of F-008).
  The install had already run at 2026-11-08T20:18:41Z, by the statement of `release-v020`.
- Evidence: the ledger of `release-v020`: `/home/flatrick/src/github/flatrick/claude-team/.scratch/agent-ledgers/release-v0.2.0/20261108T201841Z-release-v020-round6.md` (the file stamp is 20:18:41Z), and its report `/home/flatrick/src/github/flatrick/claude-team/.scratch/release-v0.2.0/report.md`.
  Re-derive the stamp: `ls /home/flatrick/src/github/flatrick/claude-team/.scratch/agent-ledgers/release-v0.2.0/`.
  The order of the leader calls: NOT RE-DERIVED (second-hand — the leader reported it; the scribe did not see the batch).
- Cause: verified by the leader's own account. A check and the action that depends on it were in one batch.
- Effect: No harm, because no other leader was online.
  The rule says to wait for each acknowledgment before the action.
- Lesson candidate (not decided): never batch the peer check with the action that depends on it.
- Status: open.
