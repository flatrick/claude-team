# The harness denied a force push that the leader had approved

- Old ID: F-013
- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: v0.2.0 release. Teammate `release-v020`. Worktree `/home/flatrick/src/github/flatrick/claude-team/.worktrees/release-v0.2.0`, branch `chore/release-v0.2.0`.
- Observation: The operator chose to rebuild the release branch after a changelog template fix.
  The leader told `release-v020` to run `git push --force-with-lease origin chore/release-v0.2.0`.
  The leader approved the push for that branch only, because no PR existed.
  The harness denied the push with `[Git Destructive]` (the teammate's ledger names the auto mode classifier).
  The teammate stopped and did not retry.
  The remote branch kept the old release commit `f5013ca`. The local branch had `0489b34` and `a6b8bd5`.
- Evidence: ledger key `rel-force-push-denied` in `/home/flatrick/src/github/flatrick/claude-team/.scratch/agent-ledgers/release-v0.2.0/20261108T201120Z-release-v020-round2.md`.
  Re-derive: `git -C /home/flatrick/src/github/flatrick/claude-team/.worktrees/release-v0.2.0 status -sb` gave `[ahead 2, behind 1]`.
  Re-derive: `git -C /home/flatrick/src/github/flatrick/claude-team/.worktrees/release-v0.2.0 rev-parse origin/chore/release-v0.2.0` gave `f5013ca85934ebef4fb75e7d39a367baae76eba5`.
  The denial happened before 20:11:41 UTC (the idle notice of the teammate).
  The scribe did not see the denial. NOT RE-DERIVED (second-hand — `release-v020` reported the denial; the denial text is from its ledger).
- Cause: verified in the ledger: the permission classifier treats a force push as destructive.
  A leader approval does not change the classifier.
- Effect: The remote branch stayed stale, and the operator got one more decision.
- Lesson candidate (not decided): a plan that rewrites a pushed branch needs the operator to push, or a new branch name.
- Status: open.
