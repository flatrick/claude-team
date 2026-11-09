# The commit-msg hook rejected a subject of 73 characters

- Date: 2026-11-09
- Claude Code version: 2.1.319 (Claude Code).
- Setup: Teammate `req-ship-team-skills`, preset `requirements-hard`, pane `%7`. Worktree `.worktrees/ship-team-skills`, branch `feat/ship-team-skills`.
- Observation: The committed `commit-msg` hook rejected a commit subject of 73 characters. The maximum is 72.
  The teammate shortened the subject and committed `63c9123`.
- Evidence: The final report of `req-ship-team-skills` gives the rejection.
  NOT RE-DERIVED (second-hand — `req-ship-team-skills` observed the rejection, not the scribe; the rejected 73-character subject is not recorded).
  The limit: `rg -n 'subjectMax' /home/flatrick/src/github/flatrick/claude-team/.girconfig` gives line 4, `subjectMax = 72`.
  The hook `.githooks/commit-msg` only calls `gir hook commit-msg`, so `rg -n '72' .githooks/` finds nothing. The limit is in `.girconfig`.
  The accepted subject: `git -C /home/flatrick/src/github/flatrick/claude-team/.worktrees/ship-team-skills log -1 --format=%s 63c9123 | awk '{print length($0)}'` printed 69 on 2026-11-09.
  The brief does not state the limit: `rg -n -i '72|subject' /home/flatrick/src/github/flatrick/claude-team/.claude/agents/requirements-hard.md` finds nothing.
- Cause: inferred, not verified. The brief of the requirements preset does not state the subject limit, so the agent learns it from the hook.
- Effect: One extra commit attempt. No work was lost.
- Case 2: Teammate `impl-ship-team-skills`, preset `implementer-standard`, pane `%9`. Same worktree.
  The `gir` commit hook rejected a commit header over 72 characters. The teammate retried one commit.
  The brief of this teammate stated "subject at most 72 characters; `.girconfig` sets `subjectMax = 72`". The limit in the brief did not prevent the first long subject.
  Evidence: `rg -n -A1 'Friction for the scribe' /home/flatrick/src/github/flatrick/claude-team/.scratch/ship-team-skills/apply-report.md` gives line 49.
  NOT RE-DERIVED (second-hand — `impl-ship-team-skills` observed the rejection, not the scribe; the rejected header is not recorded).
  The brief text is also second-hand, from the team lead: NOT RE-DERIVED (second-hand — the team lead quoted the brief; the scribe did not read it).
  The accepted subjects: `git -C /home/flatrick/src/github/flatrick/claude-team/.worktrees/ship-team-skills log --format=%s d3d8d1f..HEAD | awk '{print length($0)}'` printed 57 70 59 47 51 68 69 69 60 on 2026-11-09. All are 72 or less.
  Cause: inferred, not verified. A statement of the limit in the brief did not prevent the first long attempt.
  Effect: one extra commit attempt. No work was lost.
- Status: open
