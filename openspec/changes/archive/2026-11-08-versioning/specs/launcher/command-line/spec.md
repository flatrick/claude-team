## MODIFIED Requirements

### Requirement: Accepted options

The tool SHALL accept only these options: `--session <name>`, `--dry-run`, `-h`, `--help`, `--version`, `--install-peer-rule <target>`, `--install-leader-agent <target>`, `--force`, and `--`.
Source: `src/ClaudeTeam/Options.cs` (`Parse`), `tests/ClaudeTeam.Tests/OptionsTests.cs`.

#### Scenario: No arguments

- **WHEN** the tool runs with no arguments
- **THEN** no session override, dry run, help, version, install target, force, or forwarded argument is set

#### Scenario: Unknown option

- **WHEN** an argument before `--` is not an accepted option, for example `--unknown`
- **THEN** parsing fails with the message `Unknown option '--unknown'. Put Claude arguments after --.`

## ADDED Requirements

### Requirement: Version

The tool SHALL print one line to standard output and exit with code 0 when `--version` is given and parsing succeeds, before any install or launch.
Source: `src/ClaudeTeam/CommandResolver.cs` (`Resolve`), `src/ClaudeTeam/Program.cs`. Tests: `CommandResolverTests.cs`, `OptionsTests.cs`.

#### Scenario: Version alone

- **WHEN** the tool runs with `--version`
- **THEN** standard output shows one line that starts with `claude-team ` and the version
- **AND** no session is started
- **AND** the exit code is 0

#### Scenario: Version with an install option

- **WHEN** the tool runs with `--version --install-peer-rule repo`
- **THEN** the version line is printed
- **AND** no file is written and no session is started
- **AND** the exit code is 0

#### Scenario: Version with a parse error

- **WHEN** the tool runs with `--version --unknown`
- **THEN** parsing fails as for any unknown option
- **AND** the exit code is 2
- **AND** no version line is printed

### Requirement: Version line

The version line SHALL be `claude-team <version>`. The version is the informational version of the assembly, with the commit shortened to 7 characters, for example `claude-team 0.1.1-alpha.0.3+bf37b92`.
Source: `src/ClaudeTeam/VersionLine.cs` (`Format`, `Current`). Tests: `tests/ClaudeTeam.Tests/VersionLineTests.cs`.

#### Scenario: Full commit shortened

- **WHEN** the informational version is `0.1.1-alpha.0.3+bf37b92` followed by 33 more hexadecimal characters
- **THEN** the line is `claude-team 0.1.1-alpha.0.3+bf37b92`

### Requirement: No short alias for version

The tool SHALL accept no short alias for `--version`.
Source: `src/ClaudeTeam/Options.cs` (`Parse`, the default case). Tests: `tests/ClaudeTeam.Tests/OptionsTests.cs` (`Rejects_a_short_version_alias`).

#### Scenario: Short alias

- **WHEN** the tool runs with `-v`
- **THEN** parsing fails with the message `Unknown option '-v'. Put Claude arguments after --.`

### Requirement: Help wins over version

The tool SHALL print the help text, and not the version line, when both `--help` (or `-h`) and `--version` are given and parsing succeeds.
The help text SHALL list `--version`.
Source: `src/ClaudeTeam/CommandResolver.cs` (`Resolve`), `src/ClaudeTeam/Help.cs`. Tests: `CommandResolverTests.cs` (`Help_wins_over_version`), `HelpTests.cs`.

#### Scenario: Both flags

- **WHEN** the tool runs with `--version --help`, or with `--help --version`
- **THEN** the help text is printed
- **AND** no version line is printed
- **AND** the exit code is 0

#### Scenario: Help lists the option

- **WHEN** the tool runs with `--help`
- **THEN** the options section of the help text names `--version`
