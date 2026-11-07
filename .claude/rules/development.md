---
paths:
  - "src/**"
  - "tests/**"
---

# Development rules

These rules govern how this project is built. LLM agents and developers must follow them.

## Red–green–refactor (TDD)

- **Red**: Write the minimal test that expresses the intended behavior and watch it fail.
- **Green**: Write the minimal code to make the test pass.
- **Iterate**: Make the test more detailed step by step until it describes the full, desired behavior; keep the tests passing.
- **Refactor**: Once behavior is complete, refactor for readability, maintainability, and stability without changing behavior.

Do not add behavior without a failing test first.

## YAGNI / Keep It Simple

Do **not** introduce interfaces or abstract classes "for flexibility" until there is a concrete need
(for example two implementations, test doubles, or a clear extension point). Prefer simple, direct code.

## Verify findings before acting on them

A **finding** is a review comment, a claimed bug, a "the SDK does X" assertion, or a hypothesis about
why something behaves a certain way.
A finding is a *claim to be checked*, never a directive to obey.
Verify it against **ground truth** before you act on it, and before you commit any code that acts on it.

- **Ground truth, not authority or confidence.** Check the claim against the thing that decides it:
  the source, the SDK, a runtime observation, or a focused test.
  A confident finding can be wrong, and a tentative finding can be right.
- **Resolve, do not reflexively comply.** Apply a finding only after verification confirms it.
  Reject it with a stated reason when verification refutes it. Record both outcomes.
- **Exploration is cheap; large rewrites are not.** Validate a premise with throwaway code before
  you commit to a pattern or rewrite a large section.
  Do not commit the throwaway code as the solution.
  Keep the conclusion reproducible, for example a recorded command and its output.

## Commit quality

- **`.gitattributes`**: The repository has a root `.gitattributes`. Respect it. Do not commit changes that break line-ending or diff behavior.
- **Never use `Environment.NewLine` to match against a repository fixture file.**
  `Environment.NewLine` is `"\r\n"` on Windows. A checked-in fixture uses LF.
  A comparison built with `Environment.NewLine` never matches on Windows and passes on Linux. Use `"\n"`.
- **Hooks**: The repository has committed hooks in `.githooks/`.
  Set `git config core.hooksPath .githooks` once, and see [worktree-workflow.md](worktree-workflow.md).
- **Tests**: Every project under `tests/` runs from `ClaudeTeam.slnx`. All tests run locally.

## Secrets and configuration

- **No secrets in the repository**: Do not store API keys, license keys, or authentication information.
- **Samples only**: Commit sample configuration with placeholder values only. Make sure `.gitignore` excludes real local secret files.
