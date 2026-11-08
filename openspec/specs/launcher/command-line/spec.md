# launcher/command-line Specification

## Purpose

Defines the command-line surface of `claude-team`: which options it accepts, how it rejects invalid input, how it forwards arguments to `claude`, and how it shows help. This is a shared spec for the launcher area: every invocation of `claude-team` parses its options first, so these requirements apply to every spec under `launcher/`.

## Requirements

### Requirement: Accepted options

The tool SHALL accept only these options: `--session <name>`, `--dry-run`, `-h`, `--help`, `--install-peer-rule <target>`, `--install-leader-agent <target>`, `--force`, and `--`.
Source: `src/ClaudeTeam/Options.cs` (`Parse`), `tests/ClaudeTeam.Tests/OptionsTests.cs`.

#### Scenario: No arguments

- **WHEN** the tool runs with no arguments
- **THEN** no session override, dry run, help, install target, force, or forwarded argument is set

#### Scenario: Unknown option

- **WHEN** an argument before `--` is not an accepted option, for example `--unknown`
- **THEN** parsing fails with the message `Unknown option '--unknown'. Put Claude arguments after --.`

### Requirement: Parse errors exit with code 2

The tool SHALL write the parse error and the line `Run 'claude-team --help' for usage.` to standard error and exit with code 2 when parsing fails, and it SHALL start nothing.
Source: `src/ClaudeTeam/Program.cs` (lines 3-9).

#### Scenario: Invalid option

- **WHEN** the tool runs with an invalid argument
- **THEN** standard error shows the parse error followed by `Run 'claude-team --help' for usage.`
- **AND** the exit code is 2

### Requirement: Forward arguments after the separator

The tool SHALL treat every argument after the first `--` as an argument for `claude` and SHALL NOT parse those arguments as its own options.
Source: `src/ClaudeTeam/Options.cs` (`case "--"`), `src/ClaudeTeam/LaunchPlan.cs` (`Build`).

#### Scenario: Options after the separator

- **WHEN** the tool runs with `--dry-run -- --model opus --session x`
- **THEN** dry run is set
- **AND** `--model opus --session x` is forwarded to `claude` unchanged

### Requirement: Session name validation

The tool SHALL reject `--session` when no value follows it, when the next argument is `--`, or when the value is empty, whitespace only, or contains a control character, a period, or a colon.
Source: `src/ClaudeTeam/Options.cs` (`case "--session"`), `tests/ClaudeTeam.Tests/OptionsTests.cs` (`Rejects_invalid_input`).

#### Scenario: Missing session value

- **WHEN** the tool runs with `--session` as the last argument, or with `--session --`
- **THEN** parsing fails with the message `Supply a name after --session.`

#### Scenario: Session value with a tmux target separator

- **WHEN** the tool runs with `--session a.b`, `--session a:b`, or `--session " "`
- **THEN** parsing fails with the message `Use a nonempty session name without control characters, periods, or colons.`

### Requirement: Install target values

The tool SHALL accept `repo`, `global`, or `both`, in any letter case, as the value of `--install-peer-rule` and `--install-leader-agent`, and SHALL reject a missing value.
Source: `src/ClaudeTeam/Options.cs` (`InstallTarget`, `Enum.TryParse` with `ignoreCase: true`), `tests/ClaudeTeam.Tests/OptionsTests.cs`.

#### Scenario: Case-insensitive target

- **WHEN** the tool runs with `--install-peer-rule BOTH`
- **THEN** the peer-rule install target is `both`

#### Scenario: Missing or unknown target

- **WHEN** the tool runs with `--install-leader-agent` as the last argument, or with `--install-leader-agent elsewhere`
- **THEN** parsing fails with the message `Supply repo, global, or both after --install-leader-agent.`

#### Scenario: Both install options in one run

- **WHEN** the tool runs with `--install-peer-rule repo --install-leader-agent global`
- **THEN** both install targets are set

### Requirement: Force requires an install option

The tool SHALL reject `--force` unless `--install-peer-rule` or `--install-leader-agent` is also given.
Source: `src/ClaudeTeam/Options.cs` (lines 63-64).

#### Scenario: Force alone

- **WHEN** the tool runs with `--force` and no install option
- **THEN** parsing fails with the message `Use --force only with --install-peer-rule or --install-leader-agent.`

### Requirement: Help

The tool SHALL print the help text and exit with code 0 when `-h` or `--help` is given and parsing succeeds, before any install or launch.
The help text SHALL name the multiplexer backend for the current platform and the session name resolved for the current folder.
Source: `src/ClaudeTeam/Program.cs` (lines 16-22), `src/ClaudeTeam/Help.cs`.

#### Scenario: Help with an install option

- **WHEN** the tool runs with `--help --install-peer-rule repo`
- **THEN** the help text is printed
- **AND** no file is written and no session is started
- **AND** the exit code is 0

#### Scenario: Help shows the resolved session

- **WHEN** the tool runs with `--help --session demo`
- **THEN** the help text ends with `The session for this folder is named: demo`
