## ADDED Requirements

### Requirement: Missing-skill warning

If a team skill (`team-protocol`, `start-teammate`, or `end-teammate`) has no `SKILL.md` in `.claude/skills/<skill name>/` under the project root and none in `~/.claude/skills/<skill name>/`, the tool SHALL write one warning to standard error.
The warning SHALL name each missing skill and `claude-team --install-leader-agent <repo|global|both>`. The tool SHALL then continue the launch.
The project root is the same as for team-leader agent detection.

#### Scenario: One skill missing

- **WHEN** `end-teammate` is in neither location and the other team skills are installed
- **THEN** standard error shows one warning that names `end-teammate` and `--install-leader-agent`
- **AND** the launch continues

#### Scenario: All skills installed

- **WHEN** each team skill is in at least one location
- **THEN** no missing-skill warning is written

Source: `src/ClaudeTeam/TeamSkills.cs` (`MissingWarning`), `src/ClaudeTeam/LaunchOutput.cs` (`Preflight`), `tests/ClaudeTeam.Tests/TeamSkillsTests.cs` (`Warns_once_and_names_every_missing_skill_in_install_order`, `Names_only_the_missing_skill`, `No_warning_when_each_skill_is_in_at_least_one_location`, `No_warning_when_two_copies_differ`).

## MODIFIED Requirements

### Requirement: Warnings before a dry run

The tool SHALL write the missing-agent warning, the missing-peer-rule warning, and the missing-skill warning to standard error before the dry-run output.
The reason is that the tool checks for these files before it handles `--dry-run`.
Source: `src/ClaudeTeam/LaunchOutput.cs` (`Preflight`, `Write`), `src/ClaudeTeam/TeamSkills.cs` (`MissingWarning`), `tests/ClaudeTeam.Tests/LaunchOutputTests.cs` (`Preflight_gives_the_agent_rule_and_skill_warnings_in_that_order`, `Dry_run_writes_every_warning_before_the_dry_run_output`).

#### Scenario: Warnings before a dry run

- **WHEN** the tool runs with `--dry-run` and neither the team-leader agent nor the peer-leader rule is installed
- **THEN** both warnings are written to standard error before the dry-run output

#### Scenario: Skill warning before a dry run

- **WHEN** the tool runs with `--dry-run` and a team skill is missing
- **THEN** the missing-skill warning is written to standard error before the dry-run output
