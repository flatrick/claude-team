# Friction log

The `friction-scribe` teammate writes this file for the team-leader session.
Other sessions report friction to the leader, and the leader forwards it to the scribe.
Each entry gives the observation, the evidence, and the effect on coordination.
The status of each entry is open until the operator makes a decision.

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

## F-005 — A search with `rg --hidden` skipped the git-ignored `.worktrees/` folder

- Date: 2026-11-07
- Observation: The leader searched for a term with `rg --hidden` only.
  The `rg` command skipped the git-ignored `.worktrees/` folder.
  The leader reported an unsure "no match" to the operator.
- Evidence: `.worktrees/` is listed in `.gitignore`.
  The global CLAUDE.md already says to add `--no-ignore` when git-ignored files matter.
- Effect: The leader made a possibly false completeness claim to the operator.
- Cause: verified. `rg` skips git-ignored paths unless the command has `--no-ignore`.
- Fix: A `ripgrep` skill now exists (agent-skills branch `feat/ripgrep`, copy in `~/.claude/skills/ripgrep`).
- Status: addressed.

## F-006 — A Read in a worktree delivered rule files from two checkouts

- Date: 2026-11-08
- Observation: A session started in the main checkout. It then read two files in a worktree.
  The two Reads delivered the unscoped rules a second time, from the worktree copies.
  They also delivered rule copies from the worktree and from the main checkout.
  The main copies included a rule that the branch deleted (`dotnet/build-parallelism.md`).
- Evidence: `/home/flatrick/src/github/flatrick/claude-team/.scratch/agent-ledgers/rules-load-review/20261108T111516Z-rules-load-reviewer.md`, key `dual-checkout-rule-delivery-in-worktree`.
- Effect: A teammate sees the old main rule text next to the new worktree text.
  Rule edits of a branch do not govern sessions that started in main until the branch merges and the session restarts.
  The always-loaded rules cost double in a worktree.
- Not isolated: the reviewer did not find out which Read caused which delivery. A probe must read one file at a time.
- Cause: not verified.
- Status: open.

## F-007 — Nothing makes a subagent of a worker load the protocol skills

- Date: 2026-11-08
- Observation: The agent briefs allow short-lived subagents.
  `team-protocol` applies to "every teammate and subagent that sends messages".
  No text tells a worker to give `team-protocol`, the ledger path, or `dotnet-workflow` to a built-in subagent type.
  It is not tested whether a subagent receives the project rules.
- Evidence: `/home/flatrick/src/github/flatrick/claude-team/.scratch/agent-ledgers/rules-load-review/20261108T111516Z-rules-load-reviewer.md`, key `subagent-delivery-unspecified`.
  Method: `rg --hidden --glob '!.git/**' -n 'subagent' .claude` and a read of `team-protocol/SKILL.md`, section "Subagents".
- Effect: A subagent can run without the protocol, the ledger duty, or the build workflow.
  A probe with a general-purpose subagent must show which rules it receives.
- Cause: not verified.
- Status: open.
