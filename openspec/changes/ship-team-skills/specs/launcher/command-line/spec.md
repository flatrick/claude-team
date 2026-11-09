## ADDED Requirements

### Requirement: Help names the team skills

The help text for `--install-leader-agent` SHALL say that the option writes the team-leader agent and the skills `team-protocol`, `start-teammate`, and `end-teammate` to the same target.
Source: `src/ClaudeTeam/Help.cs`. The skill names in the help text have no code yet.

#### Scenario: Help lists the skills

- **WHEN** the tool runs with `--help`
- **THEN** the options section names `team-protocol`, `start-teammate`, and `end-teammate` in the text for `--install-leader-agent`
