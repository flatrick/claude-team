---
paths:
  - "src/**/*.cs"
---

# Named arguments for calls into this repo's own APIs

## Rule

When calling a method, constructor, or record defined under `src/` that takes **three or more
arguments**, pass every argument by name:

```csharp
// Good
var plan = LaunchPlan.Build(new LaunchInputs(
    Session: session,
    WorkingDirectory: workingDirectory,
    IsWindows: isWindows,
    InsidePane: insidePane,
    HasTeamLeaderAgent: hasTeamLeaderAgent,
    Forwarded: forwarded));

// Bad — positional, and the reader must count to know which flag is which
new LaunchInputs(session, workingDirectory, isWindows, insidePane, hasTeamLeaderAgent, forwarded)
```

This rule applies to the call site. It does not say how the *declaration* orders its parameters.
For the declaration, see [repo-conventions.md](repo-conventions.md). It gives the
required-before-optional constraint that the compiler enforces.

## Why

**Positional arguments force new parameters to the end of the list, and that is the wrong place.**
The natural home for a parameter is next to the parameters that it relates to. If appending is the only
option, a long signature drifts into arbitrary order. Every reader pays for this: the reader must
keep the declaration in mind to decode the call. Named arguments separate the two. The
declaration can order parameters by what belongs together, and the call site stays legible on its
own.

**Neighbors of the same type fail silently.** A run of consecutive `bool`s or `string`s accepts a swap or
an insertion without a compiler error. The defect then shows as wrong behavior, not as a build
failure. Named arguments keep each value bound to its parameter.

The threshold of three arguments is where the tradeoff turns. A call with two arguments is short enough to
read against its declaration. Names add noise to it and give no protection.

## Scope

**Applies to** calls into types declared under `src/`, with three or more arguments: methods,
constructors, positional-record construction, and local functions.

**Does not apply to:**

- **BCL and third-party calls.** This repo does not control their parameter names. The names
  can change in a package update, and they would bind our source to the naming of another party.
- **Calls with fewer than three arguments.**
- **LINQ operator lambdas.** `.Where(predicate: s => ...)` is legal and unreadable.
- **Operators, indexers, and deconstruction.** They have no argument names to use.
- **`tests/`.** Test code is out of scope. The arrangement of a test is often clearer positionally, and
  the large number of calls would make the real signal hard to find.

## Enforcement

Code review verifies this rule. No machine guard enforces it.

`src/` is not fully compliant. If an existing call site has no names, this does not show that
someone considered the rule and waived it.
