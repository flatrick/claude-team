---
paths:
  - "**/*.csproj"
  - "**/*.slnx"
  - "tests/**"
  - "src/**"
---

# dotnet build/test parallelism

## Rule

**Never run `dotnet build` and `dotnet test` (or any two `dotnet` commands that write to the
same output directories) in parallel** when they target projects in the same solution file.

MSBuild acquires exclusive locks on output files (`*.dll`, `*.pdb`, bin/obj directories) during
a build. A concurrent `dotnet test` that tries to read or write those same files will either
fail with a file-lock error or silently run against a stale binary.

## Required sequence

```
dotnet build  →  (wait for exit 0)  →  dotnet test --no-build
```

- Run `dotnet build` first and wait for it to complete successfully.
- Only then start `dotnet test`. Use `--no-build` if the build just succeeded.
- If the build fails, do not proceed to test. Fix the build error first.
- After you fix code, run `dotnet build` again before `dotnet test --no-build`.

## Determining if commands clash

Two `dotnet` commands clash when they share a solution or when one project appears in both
command lines, directly or through a solution file. In this repository the single solution
is `ClaudeTeam.slnx`. Two worktrees do not clash, because each worktree has its own `bin/obj`.

## Background tasks

A background task that runs `dotnet` still holds file locks.
Do not start a second `dotnet` command against the same solution until the background task
has finished and you have checked its output.

## Redirect the output to a log file

Redirect the complete output of each build and test run to a file under `.scratch/logs/`.
Do not pipe the output through `head` or `tail`.

```bash
mkdir -p .scratch/logs
dotnet build ClaudeTeam.slnx &> .scratch/logs/build-$(date +%Y%m%d-%H%M).log
dotnet test ClaudeTeam.slnx --no-build &> .scratch/logs/test-$(date +%Y%m%d-%H%M).log
```

Search the log with `rg` when a step fails. Do not run the command again to see output that you already produced.

## External contention: an IDE's background build service

A worktree that is open in an editor with a background build service can hold MSBuild worker
processes that contend with your command line build. Symptom: a `dotnet` command that never
finishes while its cumulative CPU time does not move.
Set `MSBUILDDISABLENODEREUSE=1` on your own command line invocations in such a worktree:

```bash
MSBUILDDISABLENODEREUSE=1 dotnet build ClaudeTeam.slnx
```

Run `dotnet build-server shutdown` first for a process tree that is already stuck.
