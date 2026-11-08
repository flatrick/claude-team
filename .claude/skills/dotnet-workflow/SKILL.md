---
name: dotnet-workflow
description: Build and test workflow for this repository's .NET projects. Load before the first dotnet command.
---

# .NET workflow

Use `ClaudeTeam.slnx` as the solution.
This skill is the one home of the build and test rules.

## Never run two `dotnet` commands in parallel

Never run `dotnet build` and `dotnet test` in parallel.
This also applies to any two `dotnet` commands that write to the same output directories in the same solution file.

MSBuild takes exclusive locks on output files (`*.dll`, `*.pdb`, `bin/`, `obj/`) during a build.
A concurrent `dotnet test` that reads or writes those files fails with a file-lock error.
It can also run against a stale binary and give no warning.

## Required sequence

Run each step in order, and wait for each step to finish.

1. Create the log folder: `mkdir -p .scratch/logs`.
2. Build: `dotnet build ClaudeTeam.slnx &> .scratch/logs/build-$(date +%Y%m%d-%H%M).log`.
3. Wait for exit code 0. If the build fails, do not run the test. Fix the build error first.
4. Test: `dotnet test ClaudeTeam.slnx --no-build &> .scratch/logs/test-$(date +%Y%m%d-%H%M).log`.
5. Read the end of each log file with the Read tool. Do not pipe the output through `head` or `tail`.
6. If a step fails, search the log with `rg`. Do not run the command again to see output that you already have.

Use `--no-build` only when the build just succeeded.
After you fix code, run `dotnet build` again before `dotnet test --no-build`.

## Commands that clash

Two `dotnet` commands clash when they share a solution.
They also clash when one project is in both command lines, directly or through a solution file.
In this repository the single solution is `ClaudeTeam.slnx`.
Two worktrees do not clash, because each worktree has its own `bin/obj`.

## Background tasks

A background task that runs `dotnet` still holds file locks.
Do not start a second `dotnet` command against the same solution until the background task is complete.
Check its output first.

## Redirect the output to a log file

Redirect the complete output of each build and test run to a file under `.scratch/logs/`, as the sequence shows.
Do not pipe the output through `head` or `tail`.

## External contention: the background build service of an IDE

A worktree that is open in an editor with a background build service can hold MSBuild worker processes.
These processes compete with your command-line build.
The symptom is a `dotnet` command that never finishes, and its cumulative CPU time does not change.
In such a worktree, set `MSBUILDDISABLENODEREUSE=1` on your own command-line invocations:

```bash
MSBUILDDISABLENODEREUSE=1 dotnet build ClaudeTeam.slnx
```

For a process tree that is already stuck, run `dotnet build-server shutdown` first.

## Reports

State which projects ran with every pass or fail claim.
Do not write a bare count. Give the command that produces the number.
See `.claude/rules/repo-conventions.md`.
