---
paths:
  - "tests/**"
  - "**/*.csproj"
  - "**/*.slnx"
---

# Testing discipline

## Before you mark a task complete

All three steps must pass, in this order:

1. **Clean rebuild.** Do not rely on a cached binary. Run `dotnet build ClaudeTeam.slnx`.
2. **Full test run.** Run `dotnet test ClaudeTeam.slnx --no-build`.
3. **Read the log.** Redirect the full output to a log file under `.scratch/logs/` and read it.
   Do not pipe the output through `head` or `tail`.

A run that executed zero tests is not a pass.
See `.claude/skills/dotnet-workflow/SKILL.md` for the build and test sequence.

## Do not use `--no-build` after a code fix

Never run `dotnet test --no-build` immediately after you edit source files.
The test runs against the stale binary, and the fix is not active.
A revert is also a code fix. A `--no-build` run after a revert measures the old binary.

## Flaky test verification

When you fix a flaky or intermittent test:

- Rebuild first.
- Run the test several times against the rebuilt binary. Three passing runs is the minimum.
- One green run is not evidence. You cannot tell it apart from a failure that did not recur.

## State the scope with every pass or fail claim

"The suite is green" is a claim about a set of projects.
Name what ran: the solution or the projects.
`dotnet test <project>` and `dotnet test <solution>` are different measurements.

A test that fails only in a solution-wide run is not automatically a flake.
Confirm that it passes repeatedly in isolation against the same binary, and record both measurements.

## Verify a guard by breaking what it guards

A green test proves that it passed. It does not prove that the test exercised the clause that it names.
Before you trust a test that guards a branch, a predicate, or a fallback, break that code. Confirm that the test goes red.
Record the red result next to the green result.

The loop is: mutate, build, run, revert, build, run.
Do not skip the build after the revert.

## A guard that hangs on regression is worse than no guard

A test that hangs reports no result, and it reads as a filter that matched nothing.
Two shapes cause a hang:

- An assertion that throws before it releases a held lock or signal. Capture the outcome, release, then assert.
- An unbounded wait on a signal that the regression prevents. Bound every wait, for example with `Task.WhenAny(signal, work, Task.Delay(...))`.

## An absence assertion must look in the right place

For an assertion that something is absent, write down where the thing would appear if it existed.
Confirm that the assertion looks there.
An assertion that passes on a convenient neighbor proves that the code runs, not that the guard covers it.

## Assert at the surface the requirement names

When a requirement is about the output of a command, assert on that output, not on an internal value.
Pair each "shows X" test with a "does not show X when it does not apply" test.

## Do not start the full gate until the tree is final

A full run measures the tree as it was when the run started.
An edit after the start makes the verdict describe a tree that no longer exists.
Finish the edits, then start the gate.
If something must change mid-run, stop the run and start it again.

## Do not re-run a green gate without a reason

After a full run is green, run it again only when a later commit changes compiled behavior.
A documentation edit does not qualify.
Do not repeat a run until it passes. A repeated run selects a favorable sample and tests nothing.

## Never use `Environment.NewLine` to match a repository fixture file

`Environment.NewLine` is `"\r\n"` on Windows. A checked-in fixture uses LF, because the repository is `eol=lf`.
A comparison built with `Environment.NewLine` never matches on Windows and passes on Linux. Use `"\n"`.
