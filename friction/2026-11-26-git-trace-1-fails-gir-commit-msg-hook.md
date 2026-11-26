# `GIT_TRACE=1` makes the `gir` commit-msg hook fail

- Date: 2026-11-26
- Claude Code version: 2.1.332 (Claude Code).
- Setup: Teammate `impl-precommit`, preset `implementer-standard`, pane `%2` (second-hand, from the team lead). Worktree `.worktrees/openspec-precommit`. The teammate used `GIT_TRACE` to see which hooks Git starts.
- Observation: with `GIT_TRACE=1`, the `gir` commit-msg hook fails. It prints Git trace text as the value of `gir.typesFile`.
  With `GIT_TRACE` set to an absolute file path, the hook passes.
- Evidence: the scribe ran these checks on 2026-11-26 with `gir` 0.4.0 and Git 2.56.1, in the worktree `.worktrees/friction-log`.
  1. Write a valid message: `printf 'chore: probe message\n' > <msg-file>`.
  2. `gir hook commit-msg <msg-file>` gave exit code 0.
  3. `GIT_TRACE=1 gir hook commit-msg <msg-file>` gave exit code 2. The output started with `gir: gir.typesFile:`, then a trace line for `git config --show-origin -z --type=path --get gir.typesFile`.
  4. `GIT_TRACE=<absolute-trace-file> gir hook commit-msg <msg-file>` gave exit code 0.

  These checks call the hook command directly. They do not make a commit.
  That each probe commit of the implementer failed: NOT RE-DERIVED (second-hand — `impl-precommit` observed it, not the scribe).
- Cause: inferred, not verified. `GIT_TRACE=1` makes Git write trace text to stderr. `gir` possibly reads that text as part of the result of `git config`. The scribe did not read the `gir` source.
- Effect: a commit with `GIT_TRACE=1` fails at the hook, with an error that names a setting and not the trace variable.
  In the probe of the implementer, the script continued after the failed commits. See [2026-11-26-shared-rebase-settings-moved-branch-in-probe.md](2026-11-26-shared-rebase-settings-moved-branch-in-probe.md) (second-hand).
- Status: open
