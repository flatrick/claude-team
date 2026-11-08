# tooling/install-tool/commands Specification

## Purpose

This spec defines how `scripts/install-tool.cs` finds its checkout, the commands it accepts, and what `status` and the PATH check report.
It is part of the `install-tool` sub-area of the tooling area.

## Requirements

### Requirement: Locate the checkout from the script path

The script SHALL use the parent of its own `scripts/` folder as the checkout root. It resolves this folder from its source file path, whatever the working directory is.
If that root has neither a `.git` folder nor a `.git` file, the script SHALL write `Not a git checkout: <root> has no .git directory or file.` to standard error and exit with code 1.
Source: `scripts/install-tool.cs` (`FindCheckoutRoot`, lines 36-42).

#### Scenario: Run from a linked worktree

- **WHEN** the script runs from the copy in `.worktrees/feature/scripts/`
- **THEN** it packs and installs the code of `.worktrees/feature`

### Requirement: Commands

The script SHALL accept exactly one of `status`, `install`, `reinstall`, `uninstall`, `--help`, `-h`, or `help`.
If no argument is given, the script SHALL run `status`.
For any other command, or for any extra argument, the script SHALL write `Unknown command: <arguments>` to standard error, print the usage, and exit with code 1.
The help commands SHALL print the usage, write nothing, and exit with code 0.
Source: `scripts/install-tool.cs` (lines 44-81).

#### Scenario: Extra argument

- **WHEN** the script runs with `-- install now`
- **THEN** standard error shows `Unknown command: install now`
- **AND** the exit code is 1

### Requirement: Status

The `status` command SHALL print these items:

- the installed global version of `claude-team`, or that it is not installed
- the current branch of the checkout, or `(detached HEAD)`
- a note, if the checkout has uncommitted changes
- a reminder that a running leader keeps its old build until it restarts
- the PATH check
- the usage

The `status` command SHALL write nothing and exit with code 0.
Source: `scripts/install-tool.cs` (`RunStatus`, `GetInstalledVersion`, `GetCurrentBranch`, `IsDirty`).

#### Scenario: Not installed

- **WHEN** `dotnet tool list --global` does not list `claude-team`
- **THEN** the script prints `Global tool: not installed.`

### Requirement: PATH check

If the .NET tools folder `~/.dotnet/tools` is not on `PATH`, the script SHALL print a warning with a fix for the platform.
PATH entries SHALL match after the script expands a leading `~`, resolves the full path, and trims a trailing separator.
The match SHALL ignore letter case only on Windows.
Source: `scripts/install-tool.cs` (`PrintPathHealthCheck`, `IsToolsDirOnPath`, `NormalizePathEntry`).

#### Scenario: Tilde entry

- **WHEN** `PATH` contains `~/.dotnet/tools/`
- **THEN** no PATH warning is printed
