# tooling/install-tool/commands Specification

## Purpose

Defines how `scripts/install-tool.cs` finds its checkout, which commands it accepts, and what `status` and the PATH check report. Part of the `install-tool` sub-area of the tooling area.

## Requirements

### Requirement: Locate the checkout from the script path

The script SHALL use the parent of its own `scripts/` folder, resolved from its source file path, as the checkout root, regardless of the working directory.
The script SHALL write `Not a git checkout: <root> has no .git directory or file.` to standard error and exit with code 1 when that root has neither a `.git` folder nor a `.git` file.
Source: `scripts/install-tool.cs` (`FindCheckoutRoot`, lines 36-42).

#### Scenario: Run from a linked worktree

- **WHEN** the script runs from the copy in `.worktrees/feature/scripts/`
- **THEN** it packs and installs the code of `.worktrees/feature`

### Requirement: Commands

The script SHALL accept exactly one of `status`, `install`, `reinstall`, `uninstall`, `--help`, `-h`, or `help`, and SHALL run `status` when no argument is given.
For any other command, or for any extra argument, the script SHALL write `Unknown command: <arguments>` to standard error, print the usage, and exit with code 1.
The help commands SHALL print the usage, write nothing, and exit with code 0.
Source: `scripts/install-tool.cs` (lines 44-81).

#### Scenario: Extra argument

- **WHEN** the script runs with `-- install now`
- **THEN** standard error shows `Unknown command: install now`
- **AND** the exit code is 1

### Requirement: Status

The `status` command SHALL print the installed global version of `claude-team`, or that it is not installed, the current branch of the checkout or `(detached HEAD)`, a note when the checkout has uncommitted changes, a reminder that a running leader keeps its old build until restarted, the PATH check, and the usage.
The `status` command SHALL write nothing and exit with code 0.
Source: `scripts/install-tool.cs` (`RunStatus`, `GetInstalledVersion`, `GetCurrentBranch`, `IsDirty`).

#### Scenario: Not installed

- **WHEN** `dotnet tool list --global` does not list `claude-team`
- **THEN** the script prints `Global tool: not installed.`

### Requirement: PATH check

The script SHALL print a warning with a platform-specific fix when the .NET tools folder `~/.dotnet/tools` is not on `PATH`.
PATH entries SHALL match after the script expands a leading `~`, resolves the full path, and trims a trailing separator; the match SHALL ignore letter case only on Windows.
Source: `scripts/install-tool.cs` (`PrintPathHealthCheck`, `IsToolsDirOnPath`, `NormalizePathEntry`).

#### Scenario: Tilde entry

- **WHEN** `PATH` contains `~/.dotnet/tools/`
- **THEN** no PATH warning is printed
