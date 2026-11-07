---
paths:
  - "scripts/**"
---

# Cross-platform tooling

## Rule

**Every tool in this repo MUST run on native Windows without Git Bash or WSL**, and **new tooling
MUST be a single-file C# program** (`dotnet run --file scripts/foo.cs`). Do not add `.sh`, `.bash`, `.ps1`,
`.psm1`, `.cmd`, `.bat`, or `.py` files, and do not add a shell shim over a `.cs` file.

The point is one toolchain a contributor already has: the .NET SDK. Reach for C#.

```
scripts/foo.cs          # Good — runs anywhere the SDK runs
scripts/foo.sh          # Bad  — needs a shell
scripts/foo             # Bad  — a shim whose body is `exec dotnet foo.cs`
```

Start a script with `#!/usr/bin/env dotnet` (inert on Windows, gives Unix users `./scripts/foo.cs`),
then a `//` header whose usage block shows the **runnable** command.
Launch children with `ProcessStartInfo` + `ArgumentList` and `UseShellExecute = false`; never shell out.

### Pass arguments after `--`

```
dotnet run --file scripts/foo.cs                  # no arguments: nothing to intercept
dotnet run --file scripts/foo.cs -- --quiet       # arguments: `--` is required
```

Always use `dotnet run --file`. The bare form `dotnet scripts/foo.cs` runs the project instead of
the file when a project file sits in the directory.

`dotnet run --file <file>.cs` forwards only the arguments it does not claim for itself. `--help` and `-h` are
claimed — they print .NET's own help and your script's help never runs. Worse, `--verbosity quiet`
is swallowed with **no error**, so the script runs with no arguments at all and prints a plausible,
wrong result. A flag reaching your script without `--` is luck, and the set of claimed flags grows
between SDK versions. Any usage string the script itself prints must name the `--` form too.

## Why

Nothing in this repository catches a platform break except a human on a Windows machine.
The tool targets both `tmux` and `psmux`, so a shell script would break the Windows path.
A written rule is the backstop.

## Not covered by this rule

- **Git hooks.** Git invokes hooks through a shell on every platform.
- **Environment.NewLine in tests.** Compare against repo content with `"\n"`, not `Environment.NewLine`.
  The repository is `eol=lf`, so `Environment.NewLine` is `"\r\n"` on Windows and never matches.

## Enforcement

Behavioral. Code review catches a new `.sh`, `.ps1`, or `.py` file.
