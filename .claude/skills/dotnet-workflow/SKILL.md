---
name: dotnet-workflow
description: Build and test workflow for this repository's .NET projects. Load before the first dotnet command.
---

# .NET workflow

Use `ClaudeTeam.slnx` as the solution.
This skill is the one home of the build and test rules.
Install the .NET 10 SDK.
The tool is `src/ClaudeTeam/ClaudeTeam.csproj`.
The rule `.claude/rules/worktree-workflow.md` says how to install the tool.

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

## The gate

The gate is a clean build and a full test run of `ClaudeTeam.slnx`.
Run it before you mark a task complete.
The sequence above is the gate. All steps must pass, in this order.

1. **Clean rebuild.** Do not rely on a cached binary.
2. **Full test run.** A run that executed zero tests is not a pass.
3. **Read the log.** Read the log file that you wrote under `.scratch/logs/`.

Follow these rules for each gate run:

- **Use `--no-build` only when the build just succeeded.** Never run `dotnet test --no-build` immediately after you edit source files. Run `dotnet build` again first.
  The test runs against the stale binary, and the fix is not active.
  A revert is also a code fix. A `--no-build` run after a revert measures the old binary.
- **Do not start the gate until the tree is final.** A full run measures the tree as it was when the run started.
  An edit after the start makes the verdict describe a tree that no longer exists.
  Finish the edits, then start the gate. If something must change during a run, stop the run and start it again.
- **Do not run a green gate again without a reason.** Run it again only when a later commit changes compiled behavior.
  A documentation edit does not qualify.
  Do not repeat a run until it passes. A repeated run selects a favorable sample and tests nothing.
- **Verify a fix of a flaky test.** Rebuild first.
  Run the test several times against the rebuilt binary. Three passing runs is the minimum.
  One green run is not evidence. You cannot tell it apart from a failure that did not recur.

## Pass and fail claims

"The suite is green" is a claim about a set of projects.
State which projects ran with every pass or fail claim: the solution or the projects.
`dotnet test <project>` and `dotnet test <solution>` are different measurements.
A test that fails only in a solution-wide run is not automatically a flake.
Confirm that it passes repeatedly in isolation against the same binary, and record both measurements.

Do not write a bare count. Give the command that produces the number.
See `.claude/rules/repo-conventions.md`.
