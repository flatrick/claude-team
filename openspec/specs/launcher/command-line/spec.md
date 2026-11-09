# launcher/command-line Specification

## Purpose

This spec defines the command-line surface of `claude-team`: the options it accepts, how it rejects invalid input, how it forwards arguments to `claude`, and how it shows help.
It is a shared spec for the launcher area. Every run of `claude-team` parses its options first, so these requirements apply to every spec under `launcher/`.

## Requirements

### Requirement: Accepted options

The tool SHALL accept only these options: `--session <name>`, `--dry-run`, `-h`, `--help`, `--version`, `--install-peer-rule <target>`, `--install-leader-agent <target>`, `--update <target>`, `--force`, and `--`.
Source: `src/ClaudeTeam/Options.cs` (`Parse`), `tests/ClaudeTeam.Tests/OptionsTests.cs` (`Parses_every_option_and_forwards_the_rest`, `Parses_update_in_any_letter_case`, `Update_is_null_by_default`).

#### Scenario: No arguments

- **WHEN** the tool runs with no arguments
- **THEN** no session override, dry run, help, version, install target, update target, force, or forwarded argument is set

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

The tool SHALL accept `repo`, `global`, or `both`, in any letter case, as the value of `--install-peer-rule`, `--install-leader-agent`, and `--update`.
The tool SHALL reject a missing value, a numeric value, a comma-separated list, and a value with surrounding whitespace.
Source: `src/ClaudeTeam/Options.cs` (`TryParseInstallTarget`), `tests/ClaudeTeam.Tests/OptionsTests.cs`.

#### Scenario: Case-insensitive target

- **WHEN** the tool runs with `--install-peer-rule BOTH`
- **THEN** the peer-rule install target is `both`

#### Scenario: Missing or unknown target

- **WHEN** the tool runs with `--install-leader-agent` as the last argument, or with `--install-leader-agent elsewhere`
- **THEN** parsing fails with the message `Supply repo, global, or both after --install-leader-agent.`

#### Scenario: Numeric, list, or padded target

- **WHEN** the tool runs with `--install-peer-rule 0`, `--install-peer-rule repo,global`, or `--install-peer-rule " repo "`
- **THEN** parsing fails with the message `Supply repo, global, or both after --install-peer-rule.`

#### Scenario: Both install options in one run

- **WHEN** the tool runs with `--install-peer-rule repo --install-leader-agent global`
- **THEN** both install targets are set

#### Scenario: Missing update target

- **WHEN** the tool runs with `--update` as the last argument, or with `--update elsewhere`
- **THEN** parsing fails with the message `Supply repo, global, or both after --update.`

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

### Requirement: Update excludes install options

The tool SHALL reject `--update` when `--install-peer-rule` or `--install-leader-agent` is also given.
Source: `src/ClaudeTeam/Options.cs` (`Parse`), `src/ClaudeTeam/Program.cs` (exit code 2 for a parse error), `tests/ClaudeTeam.Tests/OptionsTests.cs` (`Rejects_update_with_an_install_option`, `Force_with_only_update_stays_rejected`).

#### Scenario: Update with an install option

- **WHEN** the tool runs with `--update repo --install-leader-agent repo`
- **THEN** parsing fails with the message `Use --update without --install-peer-rule or --install-leader-agent.`
- **AND** the exit code is 2

### Requirement: Help and version before an update

The tool SHALL print the help text when help is requested, or else the version line when `--version` is given, and SHALL NOT update a file.
Source: `src/ClaudeTeam/CommandResolver.cs` (`Resolve`), `tests/ClaudeTeam.Tests/CommandResolverTests.cs` (`Help_wins_over_update`, `Version_wins_over_update`), `tests/ClaudeTeam.Tests/ProgramProcessTests.cs` (`Help_with_update_prints_help_writes_no_file_and_exits_0`).

#### Scenario: Help with update

- **WHEN** the tool runs with `--help --update repo`
- **THEN** the help text is printed
- **AND** no file is written and no session is started
- **AND** the exit code is 0

### Requirement: Help names the team skills

The help text for `--install-leader-agent` SHALL say that the option writes the team-leader agent and the skills `team-protocol`, `start-teammate`, and `end-teammate` to the same target.
Source: `src/ClaudeTeam/Help.cs` (`Text`), `tests/ClaudeTeam.Tests/HelpTests.cs` (`Leader_agent_option_names_the_three_skills`).

#### Scenario: Help lists the skills

- **WHEN** the tool runs with `--help`
- **THEN** the options section names `team-protocol`, `start-teammate`, and `end-teammate` in the text for `--install-leader-agent`

### Requirement: Help names the update option

The help text SHALL list `--update <target>` and say that it adds missing shipped files and replaces changed ones, and that `--dry-run` with it shows the changes and writes nothing.
Source: `src/ClaudeTeam/Help.cs` (`Text`), `tests/ClaudeTeam.Tests/HelpTests.cs` (`Options_section_names_update_and_dry_run_with_it`).

#### Scenario: Help lists update

- **WHEN** the tool runs with `--help`
- **THEN** the options section names `--update` and `--dry-run`
