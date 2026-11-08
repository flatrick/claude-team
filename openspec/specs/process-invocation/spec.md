# process-invocation Specification

## Purpose

This is a shared spec for the whole solution.
It defines how `claude-team` and `scripts/install-tool.cs` start other programs, such as `git`, `tmux`, `psmux`, `claude`, `dotnet`, and `ps`.
Its requirements apply to every spec in `openspec/specs/`.

## Requirements

### Requirement: Start programs without a shell

The solution SHALL start every external program directly, with shell execution turned off, and SHALL pass each argument as a separate list item, never as one joined command string.
Source: `src/ClaudeTeam/Processes.cs` (`Capture`, `Run`), `scripts/install-tool.cs` (`RunGitCapture`, `RunProcessCapture`, `RunProcess`). Find every start site with `rg -n "new ProcessStartInfo" src scripts`.

#### Scenario: Argument with a space

- **WHEN** an argument contains a space, for example a folder path `/src/my repo`
- **THEN** the started program receives it as one argument
- **AND** no shell interprets it
