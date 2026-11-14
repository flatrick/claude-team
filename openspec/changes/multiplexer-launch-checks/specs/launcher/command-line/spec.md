## MODIFIED Requirements

### Requirement: Session name validation

The tool SHALL reject `--session` when no value follows it, when the next argument is `--`, or when the value is empty or contains whitespace, a control character, a period, or a colon.
Source: `src/ClaudeTeam/Options.cs` (`case "--session"`), `tests/ClaudeTeam.Tests/OptionsTests.cs` (`Rejects_invalid_input`). The whitespace rule is in the same `case "--session"` condition; its tests are `Session_name_rejection_names_the_whitespace_rule` and `Rejects_invalid_input`.

#### Scenario: Missing session value

- **WHEN** the tool runs with `--session` as the last argument, or with `--session --`
- **THEN** parsing fails with the message `Supply a name after --session.`

#### Scenario: Session value with a tmux target separator

- **WHEN** the tool runs with `--session a.b` or `--session a:b`
- **THEN** parsing fails with the message `Use a nonempty session name without whitespace, control characters, periods, or colons.`

#### Scenario: Session value with whitespace

- **WHEN** the tool runs with `--session "my repo"` or `--session " "`
- **THEN** parsing fails with the message `Use a nonempty session name without whitespace, control characters, periods, or colons.`
- **AND** the rule applies on every platform
