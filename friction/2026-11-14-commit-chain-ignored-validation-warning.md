# A commit chain did not stop on an openspec validation warning

- Date: 2026-11-14
- Claude Code version: 2.1.325 (Claude Code)
- Setup: team `session-405f32d2`, teammate `req-mux-checks`, preset `requirements-standard`, pane `%5`.
  Worktree `.worktrees/multiplexer-launch-checks`, branch `fix/multiplexer-launch-checks`. The work was the archive of the change `multiplexer-launch-checks`.
- Observation: The teammate ran the archive sync, the validation, and the commit in one command chain.
  `openspec validate --all --strict` gave a length warning after the sync. The chain still made the archive commit.
  The teammate then fixed the spec and amended the commit before any push.
  The warning is for the requirement "Sanitize default names" in `openspec/specs/launcher/session/naming/spec.md`. The requirement was over 500 characters.
  Its long `Source:` line caused the length. The fix moved the `Source:` line below the scenarios.
  The first accounts looked different (the requirement name, and the "Source line"). They describe one event.
- Evidence:
  - The commit and the amend are in the reflog: `git -C /home/flatrick/src/github/flatrick/claude-team/.worktrees/multiplexer-launch-checks reflog -8 --format='%h %gs'` shows `36f6822 commit: docs(openspec): archive multiplexer-launch-checks` and then `fe7e298 commit (amend): docs(openspec): archive multiplexer-launch-checks`. The scribe ran it on 2026-11-14.
  - The state after the amend: `openspec validate --all --strict` in that worktree printed `Totals: 14 passed, 0 failed (14 items)` on 2026-11-14.
  - The location of the requirement and the fix: `sed -n 87,106p /home/flatrick/src/github/flatrick/claude-team/.worktrees/multiplexer-launch-checks/openspec/specs/launcher/session/naming/spec.md`. On 2026-11-14 the scribe saw the heading `### Requirement: Sanitize default names` at line 87 and the `Source:` line at line 106, after the last scenario.
    The state before the fix is not shown by this command. The team lead gave the link between the long `Source:` line and the warning.
  - The warning text, the command chain, and the exit code: NOT RE-DERIVED (second-hand — `req-mux-checks` and the team lead, not the scribe). The report is `/home/flatrick/src/github/flatrick/claude-team/.scratch/multiplexer-launch-checks/report.md`. Find the lines with `rg -n -i 'warning|amend' /home/flatrick/src/github/flatrick/claude-team/.scratch/multiplexer-launch-checks/report.md`.
  - The report does not say whether the chain used `;` or `&&`, or whether a warning gives exit code 0.
- Cause: inferred, not verified. The chain did not gate the commit on a validation without warnings.
  Open: does `openspec validate --strict` exit non-zero on a warning? Test: run it on a spec with a requirement over 500 characters, then run `echo $?`.
- Effect: A wrong commit existed for a short time. The teammate fixed it with an amend before any push. No effect on other teammates.
- Related: the same archive produced a long requirement after the sync. See the case in [2026-11-08-archive-sync-long-requirements.md](2026-11-08-archive-sync-long-requirements.md).
- Status: open
