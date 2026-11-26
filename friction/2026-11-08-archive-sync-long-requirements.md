# The archive sync made two main-spec requirements too long, and only the post-archive validation showed it

- Old ID: F-012
- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run, change `versioning`. Commands: `/opsx:archive` (`openspec archive versioning -y`) and `openspec validate`.
- Observation: `/opsx:archive` synced the delta specs into the main specs.
  Then `openspec validate --all --strict` warned "Requirement text is very long (>500 characters)" for two main specs:
  - `tooling/install-tool/commands`, requirement `Status`.
  - `tooling/install-tool/install`, requirement `Install replaces the global tool`.
  `openspec validate versioning --strict` on the change had passed before the archive.
  The length check on the merged requirement text showed only after the sync.
- Evidence: ledger entries of `versioning-implementer` (round 6) and of the requirements teammate (round 3).
  - `versioning-implementer` recorded this: "After `openspec archive versioning -y`, `openspec validate --all --strict` gave 2 warnings (>500 characters) in tooling/install-tool/commands and tooling/install-tool/install."
    Its totals were "11 passed, 2 failed (13 items)" before the fix, and "13 passed, 0 failed (13 items)" after commit `69a82e2`.
  - The requirements teammate recorded that it split each of the two requirements into two requirements.
  - NOT RE-DERIVED (second-hand — the two teammates observed this, not the scribe).
- Cause: the leader and the ledger entries state it. The scribe did not run the archive. NOT RE-DERIVED (second-hand — leader and `versioning-implementer` observed it).
- Effect: One extra teammate round and an amend of the archive commit.
- Idea (not decided): run `openspec validate --all --strict` on a trial archive before the real one.
- Case 2 (2026-11-14): change `multiplexer-launch-checks`, teammate `req-mux-checks`, branch `fix/multiplexer-launch-checks`.
  The archive sync left a length warning in the spec `launcher/session/naming`. The teammate fixed the spec and amended the archive commit `fe7e298`.
  The requirement is "Sanitize default names". Its long `Source:` line made it too long. The fix moved the `Source:` line below the scenarios (see [2026-11-14-commit-chain-ignored-validation-warning.md](2026-11-14-commit-chain-ignored-validation-warning.md)).
  Re-derive the amend: `git -C /home/flatrick/src/github/flatrick/claude-team/.worktrees/multiplexer-launch-checks reflog -8 --format='%h %gs'`.
  Check of 2026-11-25: NOT RE-DERIVED (conditions gone — `git worktree list` does not show this worktree, so its reflog command fails).
  No ref contains `fe7e298`, so there is no lasting replacement.
  The warning text itself: NOT RE-DERIVED (second-hand — `req-mux-checks`, not the scribe).
- Status: open.
