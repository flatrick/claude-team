## ADDED Requirements

### Requirement: Missing-skill warning

If a team skill (`team-protocol`, `start-teammate`, or `end-teammate`) has no `SKILL.md` in `.claude/skills/<skill name>/` under the project root and none in `~/.claude/skills/<skill name>/`, the tool SHALL write one warning to standard error.
The warning SHALL name each missing skill and `claude-team --install-leader-agent <repo|global|both>`. The tool SHALL then continue the launch.
The project root is the same as for team-leader agent detection.
Source: no code yet.

#### Scenario: One skill missing

- **WHEN** `end-teammate` is in neither location and the other team skills are installed
- **THEN** standard error shows one warning that names `end-teammate` and `--install-leader-agent`
- **AND** the launch continues

#### Scenario: All skills installed

- **WHEN** each team skill is in at least one location
- **THEN** no missing-skill warning is written

## MODIFIED Requirements

### Requirement: Warnings before a dry run

The tool SHALL write the missing-agent warning, the missing-peer-rule warning, and the missing-skill warning to standard error before the dry-run output.
The reason is that the tool checks for these files before it handles `--dry-run`.
Source: `src/ClaudeTeam/Program.cs` (lines 38-45 for the warnings, lines 50-57 for the dry run). The skill warnings have no code yet.

#### Scenario: Warnings before a dry run

- **WHEN** the tool runs with `--dry-run` and neither the team-leader agent nor the peer-leader rule is installed
- **THEN** both warnings are written to standard error before the dry-run output

#### Scenario: Skill warning before a dry run

- **WHEN** the tool runs with `--dry-run` and a team skill is missing
- **THEN** the missing-skill warning is written to standard error before the dry-run output
