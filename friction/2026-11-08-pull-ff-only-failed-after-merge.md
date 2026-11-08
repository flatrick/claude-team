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
- Status: open.
