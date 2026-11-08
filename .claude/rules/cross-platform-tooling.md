---
paths:
  - "scripts/**"
---

# Cross-platform tooling

## Rule

**Every tool in this repo MUST run on native Windows without Git Bash or WSL**, and **new tooling
MUST be a single-file C# program** (`dotnet run --file scripts/foo.cs`). Do not add `.sh`, `.bash`, `.ps1`,
`.psm1`, `.cmd`, `.bat`, or `.py` files, and do not add a shell shim over a `.cs` file.

The goal is one toolchain that a contributor already has: the .NET SDK. Use C#.

```
scripts/foo.cs          # Good — runs anywhere the SDK runs
scripts/foo.sh          # Bad  — needs a shell
scripts/foo             # Bad  — a shim whose body is `exec dotnet foo.cs`
```

Start a script with `#!/usr/bin/env dotnet` (it does nothing on Windows, and it gives Unix users `./scripts/foo.cs`).
Then write a `//` header with a usage block that shows the **runnable** command.
Start child processes with `ProcessStartInfo` + `ArgumentList` and `UseShellExecute = false`. Never start a child process through a shell.

### Pass arguments after `--`

```
dotnet run --file scripts/foo.cs                  # no arguments: nothing to intercept
dotnet run --file scripts/foo.cs -- --quiet       # arguments: `--` is required
```

Always use `dotnet run --file`. The bare form `dotnet scripts/foo.cs` runs the project instead of
the file when a project file is in the directory.

`dotnet run --file <file>.cs` forwards only the arguments that it does not claim for itself. It claims `--help` and `-h`.
They print the help of .NET, and the help of your script never runs. It also swallows `--verbosity quiet`
with **no error**. The script then runs with no arguments and prints a plausible,
wrong result. A flag that reaches your script without `--` is luck, and the set of claimed flags grows
between SDK versions. Any usage string that the script prints must also name the `--` form.

## Why

Only a human on a Windows machine can catch a platform break in this repository.
The tool targets both `tmux` and `psmux`, so a shell script would break the Windows path.
A written rule is the backstop.

## Not covered by this rule

- **Git hooks.** Git invokes hooks through a shell on every platform.
- **Environment.NewLine in tests.** Compare against repo content with `"\n"`, not `Environment.NewLine`.
  The repository is `eol=lf`, so `Environment.NewLine` is `"\r\n"` on Windows and never matches.

## Enforcement

Behavioral. Code review finds a new `.sh`, `.ps1`, or `.py` file.
