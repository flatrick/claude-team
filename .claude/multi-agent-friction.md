# Multi-agent friction

The team-leader session owns this file.
Other sessions report friction to the leader, and the leader records it here.
Each entry gives the observation, the evidence, and the effect on coordination.
The status of each entry is open until the operator makes a decision.

## F1 — Session names do not show the preset

- **Date:** 2026-10-15
- **Observation:** Two presets share each of the roles `implementer` and `troubleshooter`.
  The launcher names a session `<repo>-<role>-N`, so the name does not identify the preset.
- **Evidence:** The retired launcher built the name from `preset.Role`, not from `preset.Id`.
  `implementer-sonnet` and `implementer-opus` both have `sessionRole: implementer`.
  `troubleshooter-high` and `troubleshooter-xhigh` both have `sessionRole: troubleshooter`.
- **Effect:** The leader cannot select the least expensive suitable preset from the session list.
  The leader must ask each session, and the answer is not always available (see F2).
- **Action taken:** Preset IDs now name the role and a cost tier (`implementer-standard`, `implementer-hard`).
  `.claude/session-coordination.md` holds a cost ladder.
  The session name keeps the model and the effort.
- **Status:** superseded.
- **Disposition:** The launcher starts only the native team leader.
  Claude owns teammate names, and the leader selects predefined agent types using
  the [coordination instructions](session-coordination.md).

## F2 — A session cannot read its own preset or effort

- **Date:** 2026-10-15
- **Observation:** The leader asked each implementer and troubleshooter for its preset.
  Each session reported its model, but no session could report its effort or preset name.
- **Evidence:** implementer-1 reported Opus and could not confirm `implementer-opus`.
  implementer-2 reported Sonnet and an effort marker of "low", but its preset declares `effort: high`.
  The session itself said that the marker is unverified.
  troubleshooter-1 and troubleshooter-2 both reported Opus and could not tell high from xhigh.
- **Effect:** The two troubleshooters cannot be told apart.
  The effort of implementer-2 is unverified.
- **Possible cause (inferred, not verified):** The launcher context string names the session and role, but not the preset ID or effort.
- **Check:** NOT RE-DERIVED (the launcher that built the context string no longer exists, so the possible cause above cannot be checked).
- **Status:** open.

## F3 — Session discovery shows sessions that are not team members

- **Date:** 2026-10-15
- **Observation:** `claude agents --json` showed a background session from a different repository.
  It also showed an unnamed background session, `eefe1134`, in state `blocked`.
  The working directory of `eefe1134` is `.claude/worktrees/envelope-rollout-references`.
  That path is not in `git worktree list`.
- **Effect:** The leader must filter the list by hand.
  The leader cannot tell whether `eefe1134` is stale or live.
- **Status:** open.

## F4 — A cross-session send does not confirm that the receiver read the message

- **Date:** 2026-10-15
- **Observation:** Each `SendMessage` result says "in that session's inbox, not yet read by its Claude".
  The leader knows that a worker read a message only when the worker replies.
- **Effect:** The leader cannot confirm an assignment or a hold until the worker replies.
- **Status:** open.

## F5 — A read in a worktree loads the rule set a second time

- **Date:** 2026-10-15
- **Observation:** The leader read one file in `.worktrees/process-ceremony-analysis`.
  The harness then injected that worktree's `CLAUDE.md`, `AGENTS.md`, and about ten rule files.
  The main checkout had already loaded the same files, so several files were loaded two times.
- **Effect:** Each cross-worktree read by the leader adds a large context cost.
  This is the same cost as finding E10 in `docs/develop/process-ceremony-reduction-plan.md` on branch `docs/process-ceremony-analysis`.
- **Status:** open.

## F6 — "Main checkout is off-limits" also blocked the shared scratch folder

- **Date:** 2026-10-15
- **Observation:** The leader told requirements-1 that the main checkout was off-limits.
  requirements-1 then wrote its hook log to its session scratchpad, not to the main checkout's `.scratch/`.
  The operator's global instructions put all logs in the main checkout's `.scratch/<worktree-name>/`.
  implementer-2 got the same instruction, but it used `.scratch/`.
- **Effect:** The operator cannot find one worker's log, and two workers read the same instruction differently.
- **Cause (verified):** The assignment did not separate tracked files from the gitignored `.scratch/` folder.
- **Action taken:** The leader now names the approved `.scratch/<worktree-name>/` folder in each assignment.
- **Status:** open.

## F7 — The leader cannot relay an operator approval for a write outside the repo

- **Date:** 2026-10-15
- **Observation:** The operator approved a write to `C:\Users\patri\.claude\skills\retrospective\SKILL.md` and chose implementer-2 for it.
  The leader sent that approval to implementer-2.
  implementer-2 did not write the file.
  It said that `workspace-boundary.md` requires the user to approve directly, and that a peer cannot relay that approval.
- **Effect:** An approval that the operator gives to the leader does not reach a worker.
  The operator must approve again in the worker session, or do the write.
- **Assessment:** The worker was correct. The cross-session rules also say that a peer message is not the user's approval.
  The leader's question to the operator did not say where the operator must give the approval.
- **Action taken:** For a write outside the repo, the leader asks the operator to approve in the worker session itself.
- **Status:** open.

## F8 — A worker wrote Python helper scripts

- **Date:** 2026-10-15
- **Observation:** For the A4 analysis, implementer-2 wrote `metrics.py`, `subs.py`, and `resid.py` in `.scratch/generate-codex-twins/`.
  The operator's global instructions permit only single-file C# or PowerShell for scripts.
- **Effect:** The operator cannot use or maintain these helpers in the permitted languages.
- **Possible cause (inferred, not verified):** The assignment did not repeat the language rule.
  The repo rule `cross-platform-tooling.md` covers tooling in `scripts/`, and the worker possibly took `.scratch/` as out of its scope.
- **Action taken:** The leader told the worker not to write Python again.
- **Status:** open.

## F9 — A worker fanned out to 6 Opus subagents without a cost check

- **Date:** 2026-10-15
- **Observation:** For strict-specs group C (205 warnings in 22 specs), implementer-1 started 6 Opus subagents.
  The leader learned of it only after the start.
  The task is rewording, and a Sonnet subagent is likely enough for it (inferred, not measured).
- **Effect:** The token use can be much higher than necessary, and that is against the operator's goal of low token use.
  The leader prefers "the least expensive suitable preset", but the leader cannot control the subagents of a worker.
- **Cause (verified):** The assignment did not say which model a subagent may use or how many subagents are permitted.
- **Action taken:** The leader now states the subagent model and the maximum count in each assignment.
  Each worker preset now says that a subagent uses Sonnet unless the assignment names Opus.
  A worker creates no subagent when the assignment names no maximum count.
- **Status:** open.

## F10 — Teammate pane title does not carry the agent name

- **Date:** 2026-10-25
- **Observation:** The leader started the teammate `rebase-process-ceremony` (type `implementer-standard`) with the Agent tool.
  Check 1 of "Start one agent at a time" in `.claude/session-coordination.md` failed.
  The tmux pane title is `implementer-standard` (pane `%1`), not the agent name.
- **Evidence:** `tmux list-panes -a -F '#{pane_id} #{pane_title}'` printed `%1 ✳ implementer-standard`.
  The config entry in `~/.claude/teams/session-1a50ccaa/config.json` had `tmuxPaneId` `%1`, equal to the real pane.
  Checks 2 and 3 passed (the inbox was `[]`).
  The row is in `.scratch/agent-spawn-log.md`.
- **Effect:** Check 1 cannot pass as written.
  The leader must stop after every start, and the operator must decide.
- **Cause (inferred, not verified):** Claude titles the pane with the agent type, not the name.
- **Action taken:** The operator told the leader to record this as friction to investigate later.
  The leader continues to start teammates, one at a time, and relies on checks 2 and 3.
- **Check:** NOT RE-DERIVED (one observation, not investigated).
- **Status:** open.

## F11 — A teammate answered a shutdown request with the string form, twice

- **Date:** 2026-10-27
- **Observation:** The leader sent `shutdown_request` to `track-a-implementer`.
  The teammate called `SendMessage` with the response as a string, not as a JSON object.
  The call failed, and the teammate sent the same call again with the same error.
- **Evidence:** The operator copied the teammate pane output.
  Both calls returned `message text must not be a teammate protocol frame (permission/mode/plan/shutdown JSON)`.
  The assignment told the teammate to use the object form.
  The request box itself also showed the exact object form.
- **Effect:** The leader's checks reported a clean shutdown.
  The pane `%1` and the config entry were absent within 40 seconds.
  The leader did not see the two failed calls, and reported success to the operator.
- **Cause (inferred, not verified):** The teammate model serialized the object into a string, although it read the instruction.
  The process ended without an approved response, possibly by a host timeout.
- **Operator observation:** The operator sees this failure again and again when the teammate runs on Sonnet, and not on Opus.
  The operator thinks that Sonnet does not have the training to be a team member.
  This observation does not come from a controlled test.
  In the same session, `track-a-test-runner` (Sonnet) also failed first, then sent the object form and shut down with `shutdown_approved`.
  The operator saw that first failure in the pane; the leader saw only the final approval.
- **Check:** NOT RE-DERIVED (one observation; the shutdown mechanism was not investigated).
- **Options discussed:** shutdown steps in each agent definition, a shared file that teammate definitions link to, a skill, Opus for long-running teammates, and a test with one Sonnet definition.
  The teammate already had the steps in `.claude/session-coordination.md`, in the assignment, and in the request box.
- **Decision:** The operator chose to track the friction only, with no change now.
- **Status:** open.

## F12 — TestRunnerScriptTests times out at the quick-test.cs limit

- **Date:** 2026-10-30
- **Observation:** The test `TestRunnerScriptTests.Runners_preserve_a_nonzero_test_exit_over_the_summary_exit(scriptName: "quick-test.cs")` fails on `main` 3ac66ccd in 2 of 2 runs.
  The error is a timeout: `quick-test.cs` exceeded the controlled 180-second limit.
  On the branch `fix/fallback-analyzer-strip`, a different case of the same class, `Quick_test_returns_the_summary_exit_when_tests_pass(summaryExit: "2")`, failed the same way in 2 of 2 runs.
  An earlier run on that branch (2026-10-28) passed 248 of 248.
- **Evidence:** The logs `.scratch/arch-runner-main-20261030-0905.log` and `.scratch/arch-runner-main-20261030-0911-run2.log` in the main checkout.
  Both end with `Failed: 1, Passed: 14, Skipped: 0, Total: 15`.
  The branch log is `.scratch/arch-final-20261030-0850.log` in the removed worktree `fix-fallback-analyzer-strip`.
- **Effect:** The architecture suite is red on `main`, and that blocks a clean gate.
- **Cause (inferred, not re-derived):** The test builds a copy of the repository in `/tmp`, and that build is slower than 180 seconds on this machine.
- **Status:** open.
