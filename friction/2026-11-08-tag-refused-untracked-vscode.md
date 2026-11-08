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
