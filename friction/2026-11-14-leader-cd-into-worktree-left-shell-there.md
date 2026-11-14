# A check command left the shell in a worktree

- Date: 2026-11-14
- Claude Code version: 2.1.325 (Claude Code)
- Setup: team `session-405f32d2`, teammate `team-lead`, preset `team-leader`. The session started in the main checkout `/home/flatrick/src/github/flatrick/claude-team`.
- Observation: The leader checked the diff of a teammate. It ran one Bash command that started with `cd /home/flatrick/src/github/flatrick/claude-team/.worktrees/review-fixes && git diff --stat && ...`.
  The shell working directory stayed in the worktree after the command.
  The harness then reported "Primary working directory: .../.worktrees/review-fixes".
  The leader ran `cd` back to the main checkout in the next call.
  No write went to the wrong checkout.
  This is the trap that [worktree-workflow.md](../.claude/rules/worktree-workflow.md) names in "Do not `cd` to the repo root for read-only surveys". Here the direction is from main to a worktree.
  The rule says to use `git -C <worktree>`, an absolute path, or a subshell. The leader did not apply it.
- Evidence: NOT RE-DERIVED (conditions gone — the shell state of that call is only in the session transcript of the leader).
  To re-create it, run `cd <worktree> && git diff --stat` in the Bash tool of the leader. Then check the working directory that the harness reports for the next call.
  The rule text: `rg -n 'Do not .cd. to the repo root' .claude/rules/worktree-workflow.md`.
  The team lead reported this observation. The scribe did not observe the call.
- Cause: verified (by the team lead, not re-derived by the scribe) — the leader used `cd` in place of `git -C` or a subshell.
- Effect: None this time. A build or an edit in the next call, without a `cd` back, could have targeted the wrong checkout. The same call could also have reported a clean result for the wrong tree.
- Open questions: The rule names the main checkout as the wrong place. It does not name the opposite direction. Decide if the rule text needs to cover both directions.
- Case 2 (2026-11-14): the scribe, preset `friction-scribe`, worktree `.worktrees/friction-log`.
  To read a spec in the worktree `.worktrees/multiplexer-launch-checks`, the scribe ran one Bash command that started with `cd .../.worktrees/multiplexer-launch-checks && sed ... && git log ...`.
  The next call started in that worktree. The harness reported "Primary working directory: .../.worktrees/multiplexer-launch-checks".
  The scribe ran `cd` back to `.worktrees/friction-log` in the next call. No write went to the other worktree.
  The scribe had loaded the rule `worktree-workflow.md`. The rule was in context and the scribe did not apply it.
  The file name keeps the word "leader", because the file name is the ID of the entry. The entry now covers a teammate too.
  Re-derive the rule text: `rg -n 'Do not .cd. to the repo root' .claude/rules/worktree-workflow.md`.
  The scribe observed this case itself. The harness text is in the transcript of the scribe only: NOT RE-DERIVED (conditions gone — the shell state of that call). Re-create it with `cd <other-worktree> && pwd`.
- Status: open
