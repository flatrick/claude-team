# tooling/install-tool/uninstall Specification

## Purpose

Defines the `uninstall` command of `scripts/install-tool.cs`, which removes the user-global `claude-team` .NET tool.

## Requirements

### Requirement: Uninstall

The `uninstall` command SHALL print that nothing is to do and exit with code 0 when `claude-team` is not installed as a global tool.
Otherwise it SHALL run `dotnet tool uninstall --global claude-team` and return a nonzero exit code of that command, or print the removed version and exit with code 0.
Source: `scripts/install-tool.cs` (`RunUninstall`).

#### Scenario: Not installed

- **WHEN** the tool is not installed
- **THEN** the script prints `claude-team is not installed as a global tool. Nothing to do.`
