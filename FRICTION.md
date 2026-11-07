# Friction log

## F-001 — Copied rules name files that this repository does not have

- Date: 2026-11-07
- Observation: The untracked `.claude/rules/` files came from another repository.
  `testing-discipline.md`, `development.md`, and `dotnet/build-parallelism.md` require `scripts/run-tests.cs`, `test.runsettings`, `DotNetMcpServer.slnx`, Reqnroll E2E tests, and Stryker runs.
- Evidence: `ls scripts/run-tests.cs test.runsettings DotNetMcpServer.slnx` in the main checkout returned "No such file or directory" for all three.
- Effect: The work on `feat/claude-team-tool` could not follow these rules as written.
  It used plain `dotnet build` and `dotnet test` with logs in `.scratch/`, per the user's global CLAUDE.md.
- Cause: inferred. The files were copied from the dotnet-mcp repository without adaptation.
- Status: open. The user decides later which copied files to keep.

## F-002 — The copied `.claude/` cited scripts and paths that do not exist

- Date: 2026-11-07
- Observation: `.claude/` was copied from another repository.
  The review cited about 26 scripts and paths that do not exist here.
  Examples are `docs/`, `AGENTS.md`, `git.md`, `.claude/hooks/`, and the folders of a planning workflow.
- Evidence: `/home/flatrick/src/github/flatrick/claude-team/.scratch/claude-dir-review/report.md`.
- Effect: A reader or an agent could follow a rule that points to a missing file.
- Cause: inferred. The directory was copied without adaptation.
  This is the same cause as F-001.
- Status: addressed on `chore/adapt-claude-dir`.

## F-003 — A teammate stopped reading its inbox after its helpers created separate teams

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

## F-004 — The leader treated a successful `SendMessage` as delivery

- Date: 2026-11-07
- Observation: The leader treated a successful `SendMessage` result as delivery.
  The leader did not check the `read` flags in the inbox of the recipient.
  Six instructions went unread for about 15 minutes.
  The leader told the operator that work was in progress.
- Evidence: the inbox file `session-0f676c72/inboxes/claude-dir-cleanup.json` showed `read: false` for those messages.
  See the F-003 evidence folder.
- Effect: The operator received a wrong status. The work did not run.
- Cause: verified. The inbox file showed `read: false`.
  A successful send means that the message reached the inbox, not that the recipient read it.
- Status: open.
