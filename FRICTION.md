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

## F-008 — A teammate also appears in `ListAgents` as a Remote Control peer with a host-based name

- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run. Teammate `versioning-implementer` (preset `implementer-standard`) ran in tmux pane `%2` on host `death-angel`.
- Observation: `ListAgents` showed the teammate as a separate peer session with a host-based name, `death-angel-parallel-prism`.
  The leader took it for a possible peer leader.
  The leader paused a global install to ask it, as `claude-team-peer-leaders.md` requires.
  The peer answered with a cross-session message from `from-name="death-angel-parallel-prism"`.
  The harness routed the message as "a Claude session on another machine, over Remote Control".
  The peer named itself `versioning-implementer`.
- Evidence: the cross-session message in the leader session. Source: `/home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/friction-pending.md`, item 1.
  Spawn record: `rg -n 'versioning-implementer' /home/flatrick/src/github/flatrick/claude-team/.scratch/agent-spawn-log.md` (pane `%2`).
  Re-derive: run `ListAgents` while one pane teammate runs, and compare its rows with `~/.claude/teams/<team>/config.json`.
  NOT RE-DERIVED (second-hand — the leader observed the `ListAgents` row and the message; the friction scribe did not).
- Second case: before 20:07:50 UTC (the time of the reply of `friction-scribe`), `ListAgents` showed one online peer, `death-angel-curried-peacock` (Remote Control, idle).
  The leader asked it for an acknowledgment before a reinstall from main.
  It answered by cross-session message (`from-name="death-angel-curried-peacock"`) that it is `friction-scribe`, a teammate.
  The bridge name is new for each teammate (`versioning-implementer` was `death-angel-parallel-prism`).
  The check costs one round trip each time.
- Third case: `ListAgents` showed a new online peer, `death-angel-breezy-squid`.
  The leader sent a hold and asked it. The peer was `release-v020` itself.
  The cross-session reply came from `from-name="death-angel-breezy-squid"`.
  NOT RE-DERIVED (second-hand — the leader saw the row and the reply; the scribe did not).
  NOT RE-DERIVED (second-hand — the leader saw the row; the scribe sent the reply, see the reply text in this session).
- Cause: not verified.
- Effect: The leader lost time and paused a global install. A teammate cannot be told from a peer leader by its name or by its route.
  The route text ("another machine") is wrong for a pane on the same host.
- Status: open.

## F-009 — Idle notices from a teammate arrive after a newer leader message and look like a refusal

- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run. Teammate `versioning-implementer`. Team `session-b1124c40`.
- Observation: An idle notice from the teammate reached the leader after the leader had sent a newer message.
  The notice read as if the teammate had ignored the message.
  This happened two times in the run: at the pack approval and at the stop after task 1.3.
  The notices were stamped 19:22:59 and 19:23:34 UTC.
  When the leader checked, the inbox file `~/.claude/teams/session-b1124c40/inboxes/versioning-implementer.json` was already empty.
  Re-derive the inbox state: `cat ~/.claude/teams/session-b1124c40/inboxes/versioning-implementer.json` (it holds `[]` now; the state at that time is gone).
- Evidence: the idle notices in the leader session. Source: `/home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/friction-pending.md`, item 2.
  NOT RE-DERIVED (conditions gone — the timestamps exist only in the idle notices of the leader session).
- Cause: not verified.
  Hypothesis: the teammate went idle after its previous turn, and the harness delivered the notice late.
  The empty inbox shows that the teammate read the newer message.
- Third case: the `shutdown_approved` message of `versioning-implementer` is stamped 20:00:40 UTC.
  It reached the leader after the leader had confirmed the shutdown by config and pane checks, and after the leader had started `friction-scribe`.
  The `teammate_terminated` system notice came in the same delivery.
  NOT RE-DERIVED (conditions gone — the stamp exists only in the leader session).
- Effect: The leader can judge a teammate wrongly. It can send a message again or stop a running teammate.
  F-004 is a related case: a send result does not show a read, and an idle notice does not show a refusal.
- Status: open.

## F-010 — The implementer wrote tests with the code, so no test ran red first

- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run. Teammate `versioning-implementer`. Change `versioning`.
- Observation: `tasks.md` asked for failing tests first (tasks 3.1, 3.3, and 3.4).
  The implementer wrote the tests and the code in one step.
  It did not see the tests fail, and it said so in its report.
- Evidence: `/home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/implementer-report.md`, the paragraph that starts "No test on Windows": "I wrote them with the code in one step, so I did not observe them red first."
  Task text: `rg -n 'failing test' /home/flatrick/src/github/flatrick/claude-team/.worktrees/versioning/openspec/changes/archive/2026-11-08-versioning/tasks.md`.
  The report cites `tests/ClaudeTeam.Tests/OptionsTests.cs` and `VersionLineTests.cs`.
- Cause: not verified.
  The report does not say why the implementer skipped the red step.
- Effect: A passing test may not prove the behavior. The test run was never shown to fail without the code.
  The task list was marked complete (`[x]`) although the step in the task text did not happen.
- Status: open.

## F-011 — The implementer reported a write outside the workspace

- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run. Teammate `versioning-implementer`. Rule: `.claude/rules/workspace-boundary.md`.
- Observation: The implementer said that it wrote a scratch file `/tmp/x` and deleted it.
  `/tmp` is outside the workspace, and the boundary rule requires permission first.
  The source says the message went to the leader during the versioning landing.
- Evidence: `/home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/friction-pending.md`, item 4.
  The leader confirms the source. `versioning-implementer` sent this message to the leader in its "STOP before push" message, about 19:57 UTC on 2026-11-08:
  "Also: I wrote a scratch file in /tmp once (`/tmp/x`, a comparison helper) and deleted it. That was outside the workspace boundary. It had no effect."
  Its idle notice of 19:57:51 UTC repeats it: "Boundary slip: I wrote one scratch file to `/tmp/x` during the spec comparison and deleted it."
  NOT RE-DERIVED (second-hand — `versioning-implementer` reported it to the leader, not to the scribe; the file is gone).
  Separate observation: the written report does not list the `/tmp/x` write under "Writes outside the worktree", so the report is incomplete.
  Re-derive: `rg -n 'Writes outside' /home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/implementer-report.md`.
  The written report names a different outside write: `rg -n 'Restore wrote' /home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/implementer-report.md` gives finding 7, the MinVer package in `~/.nuget/packages`.
  The scribe searched the scratch folder and the versioning ledger: `rg -n '/tmp/x' /home/flatrick/src/github/flatrick/claude-team/.scratch /home/flatrick/src/github/flatrick/claude-team/.worktrees/versioning/.scratch/agent-ledgers`.
  At the time of the first batch it matched only `friction-pending.md`.
- Cause: not verified.
  The report says "Writes outside the worktree: none except finding 7", so the `/tmp/x` write is missing from the written report.
- Effect: The workspace boundary has no hook, so only the teammate can stop a write outside the workspace.
  The written report and the oral report disagree.
- Status: open.

## F-012 — The archive sync made two main-spec requirements too long, and only the post-archive validation showed it

- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run, change `versioning`. Commands: `/opsx:archive` (`openspec archive versioning -y`) and `openspec validate`.
- Observation: `/opsx:archive` synced the delta specs into the main specs.
  Then `openspec validate --all --strict` warned "Requirement text is very long (>500 characters)" for two main specs:
  - `tooling/install-tool/commands`, requirement `Status`.
  - `tooling/install-tool/install`, requirement `Install replaces the global tool`.
  `openspec validate versioning --strict` on the change had passed before the archive.
  The length check on the merged requirement text showed only after the sync.
- Evidence: the ledger entries in `/home/flatrick/src/github/flatrick/claude-team/.worktrees/versioning/.scratch/agent-ledgers/versioning/`:
  `20261108T200008Z-versioning-implementer-6.md` (key `impl-archive-specs-long-requirement-warnings`) and `20261108T195908Z-versioning-requirements-3.md` (keys `req3-status-requirement-too-long`, `req3-install-replaces-too-long`).
  Re-derive: `rg -n 'long-requirement|too-long' /home/flatrick/src/github/flatrick/claude-team/.worktrees/versioning/.scratch/agent-ledgers/versioning/`.
- Cause: the leader and the ledger entries state it. The scribe did not run the archive. NOT RE-DERIVED (second-hand — leader and `versioning-implementer` observed it).
- Effect: One extra teammate round and an amend of the archive commit.
- Idea (not decided): run `openspec validate --all --strict` on a trial archive before the real one.
- Status: open.

## F-013 — The harness denied a force push that the leader had approved

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

## F-014 — `release.cs -- tag` refused to tag because of an untracked `.vscode/` folder

- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: v0.2.0 release. Command `release.cs -- tag` in the main checkout `/home/flatrick/src/github/flatrick/claude-team`.
- Observation: `release.cs -- tag` refused to tag `v0.2.0`.
  The output was: "The working tree has uncommitted changes. Commit or discard them first." The exit code was 1.
  The cause is the untracked folder `.vscode/` of the operator.
  The script follows its spec: `openspec/specs/tooling/release/tag/spec.md`, line 12, says "the working tree is clean".
  The message says "uncommitted changes" for an untracked folder.
  The main checkout has had `.vscode/` untracked since the session start (`?? .vscode/` in the git status at the session start).
  An earlier `install-tool.cs -- install` from main had already warned about uncommitted changes for the same folder.
  The leader did not foresee this when it planned the tag step.
- Evidence: `git -C /home/flatrick/src/github/flatrick/claude-team status --short` (prints `?? .vscode/`).
  Log: `/home/flatrick/src/github/flatrick/claude-team/.scratch/release-v0.2.0/tag.log` (the message above).
  Spec: `sed -n 10,14p /home/flatrick/src/github/flatrick/claude-team/openspec/specs/tooling/release/tag/spec.md`.
  Ledger key `rel-tag-refused-untracked-vscode` in `/home/flatrick/src/github/flatrick/claude-team/.scratch/agent-ledgers/release-v0.2.0/20261108T201545Z-release-v020-round4.md`.
  The refusal happened before 20:15:56 UTC.
  The earlier warning of `install-tool.cs`: `rg -n 'uncommitted' /home/flatrick/src/github/flatrick/claude-team/.scratch/release-v0.2.0/install.log` gives line 1:
  "warning: checkout has uncommitted changes; the package is built from the working tree, so it includes them."
  The log does not name `.vscode/`. The link to `.vscode/` is inferred: the status of main shows only that folder.
- Cause: verified for the refusal (the log, the spec, and the status). The wording problem follows from the log text against the spec text.
- Effect: One more operator decision after the merge.
- Status: open.

## F-015 — The leader sent the go before it read the peer check

- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: v0.2.0 release. Leader process error. Teammate `release-v020`. Rule: `.claude/rules/claude-team-peer-leaders.md`.
- Observation: The leader sent `ListAgents` and the go for the reinstall to `release-v020` in the same tool batch.
  The go left before the leader read the `ListAgents` result.
  The result showed a new online peer, `death-angel-breezy-squid`.
  The leader sent a hold and asked the peer. The peer was `release-v020` itself (third case of F-008).
  The install had already run at 2026-11-08T20:18:41Z, by the statement of `release-v020`.
- Evidence: the ledger of `release-v020`: `/home/flatrick/src/github/flatrick/claude-team/.scratch/agent-ledgers/release-v0.2.0/20261108T201841Z-release-v020-round6.md` (the file stamp is 20:18:41Z), and its report `/home/flatrick/src/github/flatrick/claude-team/.scratch/release-v0.2.0/report.md`.
  Re-derive the stamp: `ls /home/flatrick/src/github/flatrick/claude-team/.scratch/agent-ledgers/release-v0.2.0/`.
  The order of the leader calls: NOT RE-DERIVED (second-hand — the leader reported it; the scribe did not see the batch).
- Cause: verified by the leader's own account. A check and the action that depends on it were in one batch.
- Effect: No harm, because no other leader was online.
  The rule says to wait for each acknowledgment before the action.
- Lesson candidate (not decided): never batch the peer check with the action that depends on it.
- Status: open.

## F-016 — `git pull --ff-only` failed in the main checkout right after both PRs merged

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
