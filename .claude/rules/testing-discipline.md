---
paths:
  - "tests/**"
---

# Testing discipline

This rule covers how to write a test. Run the build and test gate as `.claude/skills/dotnet-workflow/SKILL.md` says.

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

## Never use `Environment.NewLine` to match a repository fixture file

`Environment.NewLine` is `"\r\n"` on Windows. A checked-in fixture uses LF, because the repository is `eol=lf`.
A comparison built with `Environment.NewLine` never matches on Windows and passes on Linux. Use `"\n"`.
