---
name: dotnet-workflow
description: Build and test workflow for this repository's .NET projects.
---

# .NET workflow

Use `ClaudeTeam.slnx` as the solution.
Run each step in order, and wait for each step to finish.
Do not run two `dotnet` commands against the solution at the same time.
See `.claude/rules/dotnet/build-parallelism.md`.

1. Create the log folder: `mkdir -p .scratch/logs`.
2. Build: `dotnet build ClaudeTeam.slnx &> .scratch/logs/build-$(date +%Y%m%d-%H%M).log`.
3. Test: `dotnet test ClaudeTeam.slnx --no-build &> .scratch/logs/test-$(date +%Y%m%d-%H%M).log`.
4. Read the end of each log file with the Read tool. Do not pipe the output through `head` or `tail`.
5. When a step fails, search the log with `rg`. Do not run the command again to see output that you already have.

State which projects ran with every pass or fail claim.
Do not write a bare count. Give the command that produces the number.
See `.claude/rules/repo-conventions.md`.
