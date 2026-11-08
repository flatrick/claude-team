---
name: dotnet-testing
description: TDD workflow for the tests in this repository.
---

# Testing workflow

Tests live in `tests/ClaudeTeam.Tests` and use xUnit.
Run them as `.claude/skills/dotnet-workflow/SKILL.md` says.

1. Write a failing test for the behavior. Run it and confirm that it fails for the right reason.
2. Write the minimal code that makes the test pass.
3. Refactor. Do not change the behavior.
4. Break the code that the test guards. Confirm that the test goes red. Revert the break and rebuild.
5. Run the full solution before you mark the task complete.

A test that needs a real directory uses `Directory.CreateTempSubdirectory`.
See `.claude/rules/test-temp-paths.md`.
Read `.claude/rules/testing-discipline.md` before you record a pass claim.

## Definition of done

1. `dotnet build ClaudeTeam.slnx` succeeds.
2. `dotnet test ClaudeTeam.slnx --no-build` passes. The log shows that tests ran.
3. The final report names the projects that ran.
