# tooling/install-tool/uninstall Specification

## Purpose

This spec defines the `uninstall` command of `scripts/install-tool.cs`. The command removes the user-global `claude-team` .NET tool.

## Requirements

### Requirement: Uninstall

If `claude-team` is not installed as a global tool, the `uninstall` command SHALL print that nothing is to do and exit with code 0.
Otherwise, the command SHALL run `dotnet tool uninstall --global claude-team`.
If that command returns a nonzero exit code, the `uninstall` command SHALL return the same code. If not, it SHALL print the removed version and exit with code 0.
Source: `scripts/install-tool.cs` (`RunUninstall`).

#### Scenario: Not installed

- **WHEN** the tool is not installed
- **THEN** the script prints `claude-team is not installed as a global tool. Nothing to do.`
