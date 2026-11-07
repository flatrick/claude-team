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

This applies to the call site. It says nothing about how the *declaration* orders its parameters —
see [repo-conventions.md](repo-conventions.md) for the required-before-optional constraint the
compiler enforces there.

## Why

**Positional arguments force new parameters to the end of the list, and that is the wrong place.**
The natural home for a parameter is next to the ones it relates to. When appending is the only
option, a long signature drifts into arbitrary order, and every reader pays for it — the reader has
to hold the declaration in their head to decode the call. Named arguments decouple the two: the
declaration can order parameters by what belongs together, and the call site stays legible on its
own.

**Same-typed neighbors fail silently.** A run of consecutive `bool`s or `string`s accepts a swap or
an insertion without a compiler error, so the defect surfaces as wrong behavior rather than a build
failure. Named arguments keep each value bound to its parameter.

The three-argument threshold is where the tradeoff turns: a two-argument call is short enough to
read against its declaration, and naming it adds noise without buying protection.

## Scope

**Applies to** calls into types declared under `src/`, with three or more arguments — methods,
constructors, positional-record construction, and local functions.

**Does not apply to:**

- **BCL and third-party calls.** Their parameter names are not a contract this repo controls; they
  can change under a package bump and would bind our source to someone else's naming.
- **Calls with fewer than three arguments.**
- **LINQ operator lambdas** — `.Where(predicate: s => ...)` is legal and unreadable.
- **Operators, indexers, and deconstruction**, which have no argument names to use.
- **`tests/`.** Test code is out of scope; a test's arrangement is often clearer positionally, and
  the sheer call volume would swamp the signal.

## Enforcement

Verified in code review. No machine guard enforces this rule.

`src/` is not uniformly compliant. The absence of names at an existing call site does not signal
that the rule was considered and waived.
