# Shared rebase settings let a probe rebase move another branch and write to the shared stash

- Date: 2026-11-26
- Claude Code version: 2.1.332 (Claude Code).
- Setup: Teammate `impl-precommit`, preset `implementer-standard`, pane `%2`. Worktree `.worktrees/openspec-precommit`, branch `chore/openspec-precommit`. The task was to add `.githooks/pre-commit`.
  One criterion of the assignment (criterion 4) required a rebase probe. It allowed throwaway commits on a throwaway branch.
  NOT RE-DERIVED (second-hand — the team lead gave the pane and the content of the criterion; the scribe did not read the assignment).
- Observation:
  - `.git/config`, which all worktrees share, sets `rebase.autosquash`, `rebase.autostash`, and `rebase.updaterefs` to `true`.
    `git config --show-origin --get-regexp '^rebase\.'` showed all three on 2026-11-26.
  - A rebase in the probe moved `chore/openspec-precommit` from `69d556b` to `60adc10`. The two commits have the same tree.
    `git reflog --date=iso chore/openspec-precommit` shows the entry "rewritten during rebase" at 17:44:16 +0100, and a reset to `69d556b` at 17:44:44.
    `git diff --stat 69d556b 60adc10` printed nothing on 2026-11-26. This check works only while Git keeps the object `60adc10`.
  - The rebase also made an autostash entry in the stash that all worktrees share.
    `git log -1 --format='%h %ci %s' be05924f` printed `be05924 2026-11-26 17:44:17 +0100 On tmp/precommit-probe: autostash`. This check works only while Git keeps the object.
  - The implementer reported these repairs: it reset the branch with `git branch -f`, dropped only its own stash entry by SHA, and deleted its throwaway branches.
    NOT RE-DERIVED (second-hand — `impl-precommit` ran the repairs, not the scribe).
  - State after the repair, on 2026-11-26: `git stash list` printed nothing, and `git branch --list 'tmp/*'` printed nothing.
    `git worktree list` showed `chore/openspec-precommit` at `69d556b`.
    `git reflog -5 --date=iso main` showed no entry from the probe rebase.
- Evidence: the commands above, and the final report of `impl-precommit`.
  The report also said that the first probe run had two errors. `GIT_TRACE=1` made each probe commit fail, and the script did not stop.
  Thus `git rebase --force-rebase HEAD~2` included `69d556b`.
  NOT RE-DERIVED (second-hand — `impl-precommit` ran the probe, not the scribe; the probe script and the throwaway branches are not available).
  For the `GIT_TRACE=1` failure, see [2026-11-26-git-trace-1-fails-gir-commit-msg-hook.md](2026-11-26-git-trace-1-fails-gir-commit-msg-hook.md).
- Cause: inferred, not verified. The probe script did not stop after the failed commits, so the rebase range included an existing commit. This is the cause that the implementer gave.
  The shared settings then did the rest: `rebase.updateRefs` moved the branch, and `rebase.autostash` made the stash entry.
  `git help rebase` (Git 2.56.1) says that `--update-refs` moves branches that point to rebased commits, but not branches that a worktree has checked out.
  The stash message names `tmp/precommit-probe`. This indicates that no worktree had `chore/openspec-precommit` checked out at that time.
- Effect: a probe in one worktree moved a different branch and wrote to the stash that all worktrees share. The implementer repaired both. No lost work was reported.
  The same setting applies to the autosquash step in `.claude/rules/repo-conventions.md` ("Fix an earlier commit with a fixup commit").
  Find the step with `rg -n 'autosquash' .claude/rules/repo-conventions.md`.
  By the Git documentation, that step can also move another local branch that points to a commit in the range, if no worktree has the branch checked out.
  The scribe did not test this with a rebase.
  A rebase with `git -c rebase.autostash=false -c rebase.updateRefs=false` turns off the two settings for one command. The implementer reported that it used this form for later rebases (second-hand).
- Open questions: Who set the three settings in `.git/config`, and does the repository need them?
- Status: open
