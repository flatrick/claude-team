# A search with `rg --hidden` skipped the git-ignored `.worktrees/` folder

- Old ID: F-005
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
