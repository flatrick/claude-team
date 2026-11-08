# A teammate stopped reading its inbox after its helpers created separate teams

- Old ID: F-003
- Date: 2026-11-07
- Claude Code version: 2.1.319.
- Setup: team `session-0f676c72`. Teammate `claude-dir-cleanup` (preset `implementer-standard`, pane `%2`, in-process).
- Observation: The teammate stopped reading its inbox.
  Its helper subagents created separate teams (`session-508a947a`, `session-db5fef1a`).
  Its final report went to `~/.claude/teams/default/inboxes/team-lead.json`.
- Timeline (times are UTC on 2026-11-07):
  - 15:29 The leader started the teammate. The three start checks passed.
  - 15:30 The teammate read a leader message. The message was marked read.
  - 15:36:27 Two teams appeared: `session-508a947a` (cwd: the main checkout, pid 99962) and `session-db5fef1a` (cwd: the worktree `.claude/rules`, pid 99961). Both have the same `procStart`. Each has only a `team-lead` member.
  - 15:36 The leader got a message labelled `team-lead` with `verified="false"` and the content of the teammate.
  - 15:36:47 onward: all leader messages stay `read: false` in `session-0f676c72/inboxes/claude-dir-cleanup.json`.
  - 15:41:58 The `idle_notification` of the teammate went to `teams/default/inboxes/team-lead.json`, not to `session-0f676c72`.
  - 15:49:57 A `shutdown_request` was not read. The leader then used `TaskStop`. The time is not recorded; it was before 15:53.
  - Before 15:53 the folder of `session-db5fef1a` was deleted, between the first read of the leader and its evidence copy. Its config is reconstructed from leader output.
- Cause: inferred, not verified.
  Hypothesis: the two helper subagents that the teammate was allowed to start began as separate sessions.
  They claimed `team-lead` of new teams, and the teammate lost its binding to `session-0f676c72`.
- Open questions:
  - What started pids 99961 and 99962?
  - Why did the teammate write to team `default`?
  - Did `EnterWorktree` in the teammate change its team context? Its status line showed `wt:adapt-claude-dir`.
- Collect next time, before any `TaskStop` or cleanup:
  - `tmux capture-pane -p -t <pane id>` of the teammate.
  - A copy of `~/.claude/teams/` (every team folder, config files, and inboxes), with timestamps.
  - The `read` flags of the teammate inbox file.
  - `ps` output for the pids and the parent pids of any new `claude` process, with their cwd.
  - The output of `claude --version`.
- Evidence (local only, gitignored): `/home/flatrick/src/github/flatrick/claude-team/.scratch/friction-evidence/F-003-inbox-routing-20261107/`.
  It holds `TIMELINE.md`, `claude-version.txt`, the pane capture, the team config before the stop, and the inbox files.
  Earlier captures: `/home/flatrick/src/github/flatrick/claude-team/.scratch/claude-dir-cleanup/pane-capture-*.log`.
- Effect: The leader could not send messages to the teammate. The leader replaced the teammate.
- Status: open — cause not verified.
