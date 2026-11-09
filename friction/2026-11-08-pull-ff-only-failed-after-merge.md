# `git pull --ff-only` failed in the main checkout right after both PRs merged

- Old ID: F-016
- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: main checkout `/home/flatrick/src/github/flatrick/claude-team`, branch `main`. Both friction PRs (#19 and #20) had just merged on GitHub. Remote `origin/main` moved to `c7c7a82`.
- Observation: The leader ran `git pull --ff-only` in the main checkout.
  Git printed "fatal: Cannot fast-forward to multiple branches."
  The fetch part worked, and `origin/main` moved to `c7c7a82`.
  `git merge --ff-only origin/main` then worked.
  The time was before 20:28:53 UTC (the `date -u` of the leader after the pull and the fast-forward).
- Evidence: `git -C /home/flatrick/src/github/flatrick/claude-team config --get-all branch.main.merge` prints only `refs/heads/main`.
  The scribe ran it and got that output.
  `git config --get-regexp '^branch\.'` in the friction-log worktree lists `branch.<name>.vscode-merge-base` keys for `main`, `docs/friction-log`, and `docs/friction-scribe-agent`. The scribe ran it.
  The error itself: NOT RE-DERIVED (second-hand — the leader saw it; the pull state changed after the merge, so the conditions are gone).
- Cause: not verified.
  A possible lead (not tested): GitHub deleted the merged remote branches, and VS Code added `vscode-merge-base` keys for `main` and both docs branches.
- Effect: One failed command and one workaround. The pull step of the after-merge flow can fail even when the fetch works.
- Case 2 (2026-11-09): same checkout, branch `main`. PR #22 (`feat/ship-team-skills`) had just merged on GitHub with rebase-and-merge.
  The leader ran `git fetch -q origin && git pull --ff-only -q`. Git printed "fatal: Cannot fast-forward to multiple branches."
  `git config --get-all branch.main.merge` printed only `refs/heads/main`. `git merge --ff-only origin/main` then worked. `main` moved to `4d786ae`.
  The leader saw the error and the config output: first-hand for the leader, second-hand for the scribe.
  The error: NOT RE-DERIVED (conditions gone — `main` is now up to date, so a pull has nothing to fast-forward).
  Config re-derivation: `git -C /home/flatrick/src/github/flatrick/claude-team config --get-all branch.main.merge` printed `refs/heads/main` when the scribe ran it on 2026-11-09.
  `git -C /home/flatrick/src/github/flatrick/claude-team config --get-regexp '^branch\.'` on 2026-11-09 shows `branch.main.remote origin`, `branch.main.merge refs/heads/main`, and `branch.main.vscode-merge-base origin/main`.
  It also shows `vscode-merge-base` keys for `docs/friction-log` and `feat/ship-team-skills`. The branches `docs/decision-option-letters` and `docs/commit-conventions` have `remote` and `merge` keys only.
  `git config --get-regexp '^(pull|fetch)\.'` shows `pull.ff only` and `fetch.prune true`.
  Result for the lead of case 1: the `vscode-merge-base` key for `main` still exists after the failure, and `branch.main.merge` has one value. The config alone does not explain the error. The lead is not confirmed and not excluded.
  This is the second failure in two days, each after a rebase-and-merge of a PR.
  Effect: one failed command and the same workaround.
- Status: open.
