# `release.cs -- tag` refused to tag because of an untracked `.vscode/` folder

- Old ID: F-014
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
  Check of 2026-11-25: this command now prints nothing. Line 7 of `.git/info/exclude` is `.vscode/`.
  `git check-ignore -v .vscode` in the main checkout prints `.git/info/exclude:7:.vscode/	.vscode`.
  `git status --short --ignored .vscode` prints `!! .vscode/`. Thus the folder exists, and Git now ignores it.
  The report of `release-v020` said that the operator approved this line. NOT RE-DERIVED (second-hand — `release-v020`, not the scribe).
  The earlier output `?? .vscode/`: NOT RE-DERIVED (conditions gone — the local exclude file now hides the folder).
  The log of the tag command held this line: "The working tree has uncommitted changes. Commit or discard them first."
  Spec: `sed -n 10,14p /home/flatrick/src/github/flatrick/claude-team/openspec/specs/tooling/release/tag/spec.md`.
  The ledger of `release-v020` (key `rel-tag-refused-untracked-vscode`) recorded exit code 1, `?? .vscode/` in `git status -sb`, and HEAD `698a881`. It recorded that no tag `v0.2.0` existed after the command.
  NOT RE-DERIVED (second-hand — `release-v020` ran the command, not the scribe).
  The refusal happened before 20:15:56 UTC.
  The earlier warning of `install-tool.cs` was line 1 of an install log:
  "warning: checkout has uncommitted changes; the package is built from the working tree, so it includes them."
  The two log lines: NOT RE-DERIVED (conditions gone — the logs were temporary files).
  The log does not name `.vscode/`. The link to `.vscode/` is inferred: the status of main shows only that folder.
- Cause: verified for the refusal (the log line, the spec, and the status). The wording problem follows from the log text against the spec text.
- Effect: One more operator decision after the merge.
- Status: open.
