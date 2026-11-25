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
- Evidence: a ledger entry and the report of `release-v020`. The file name of the entry had the stamp `20261108T201841Z`.
  The entry said: "The global tool is now claude-team 0.2.0 built from main at 698a881."
  It gave the command `dotnet run --file scripts/install-tool.cs -- install` with exit code 0, and the output "Installed claude-team 0.2.0 (commit 698a881, branch main)."
  NOT RE-DERIVED (second-hand — `release-v020` ran the install, not the scribe). The stamp is the time of the entry. The entry gave no separate time for the install.
  The order of the leader calls: NOT RE-DERIVED (second-hand — the leader reported it; the scribe did not see the batch).
- Cause: verified by the leader's own account. A check and the action that depends on it were in one batch.
- Effect: No harm, because no other leader was online.
  The rule says to wait for each acknowledgment before the action.
- Lesson candidate (not decided): never batch the peer check with the action that depends on it.
- Status: open.
