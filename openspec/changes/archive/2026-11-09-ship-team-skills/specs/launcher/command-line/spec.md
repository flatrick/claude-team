## MODIFIED Requirements

### Requirement: Accepted options

The tool SHALL accept only these options: `--session <name>`, `--dry-run`, `-h`, `--help`, `--version`, `--install-peer-rule <target>`, `--install-leader-agent <target>`, `--update <target>`, `--force`, and `--`.
Source: `src/ClaudeTeam/Options.cs` (`Parse`), `tests/ClaudeTeam.Tests/OptionsTests.cs` (`Parses_every_option_and_forwards_the_rest`, `Parses_update_in_any_letter_case`, `Update_is_null_by_default`).

#### Scenario: No arguments

- **WHEN** the tool runs with no arguments
- **THEN** no session override, dry run, help, version, install target, update target, force, or forwarded argument is set

#### Scenario: Unknown option

- **WHEN** an argument before `--` is not an accepted option, for example `--unknown`
- **THEN** parsing fails with the message `Unknown option '--unknown'. Put Claude arguments after --.`

### Requirement: Install target values

The tool SHALL accept `repo`, `global`, or `both`, in any letter case, as the value of `--install-peer-rule`, `--install-leader-agent`, and `--update`.
The tool SHALL reject a missing value, a numeric value, a comma-separated list, and a value with surrounding whitespace.
Source: `src/ClaudeTeam/Options.cs` (`InstallTarget`, `TryParseInstallTarget`), `tests/ClaudeTeam.Tests/OptionsTests.cs` (`Parses_update_in_any_letter_case`, `Rejects_numeric_list_and_padded_install_targets`, `Rejects_a_missing_or_unknown_update_target`).

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

## ADDED Requirements

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
