# launcher/starter-files/content Specification

## Purpose
This spec defines the content of the team files that `claude-team` ships: the team-leader agent and the team skills `team-protocol`, `start-teammate`, and `end-teammate`.
It covers the local extension point, the load confirmations, the neutral text, and the multiplexer commands in the skills.

## Requirements

### Requirement: Local extension line

The first line in the body of each shipped team file, after the front matter, SHALL be: "Invoke the skill `<own name>-local`. If the tool says `Unknown skill`, continue."
`<own name>` is `team-leader`, `team-protocol`, `start-teammate`, or `end-teammate`.
A skill call works before the skill listing of a teammate arrives, and a missing skill gives `Unknown skill`. So the line needs no test for existence.

#### Scenario: Shipped skill body

- **WHEN** the embedded `start-teammate` skill is read
- **THEN** the first line after its front matter is "Invoke the skill `start-teammate-local`. If the tool says `Unknown skill`, continue."

#### Scenario: Shipped agent body

- **WHEN** the embedded team-leader agent is read
- **THEN** the first line after its front matter names `team-leader-local`

Source: `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`First_body_line_invokes_the_local_skill`, one case for the agent and for each skill), `src/ClaudeTeam/team-leader.md`, `src/ClaudeTeam/skills/*/SKILL.md`.

### Requirement: Local skills stay in the repository

The shipped files SHALL describe a `<name>-local` skill as a file that a repository creates in its own `.claude/skills/`, only for the shipped files that it extends.
The shipped files SHALL NOT tell the user to put a `-local` skill in the user profile.
A skill in the user profile hides a repository skill of the same name, so a user profile `-local` skill would replace the extension of each repository.

#### Scenario: Where a local skill lives

- **WHEN** the embedded `team-protocol` skill is read
- **THEN** it says that a `-local` skill belongs in the `.claude/skills/` folder of the repository

Source: `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`Protocol_gives_both_load_lines_and_the_local_skill_location`, `Protocol_forbids_a_local_skill_in_the_user_profile_and_says_why`, `Shipped_text_never_tells_the_user_to_put_a_local_skill_in_the_user_profile`), `src/ClaudeTeam/skills/team-protocol/SKILL.md`.

### Requirement: Teammate load confirmation

The shipped `team-protocol` skill SHALL tell a teammate to start its first message to the team lead with the line `team-protocol loaded`.
If the teammate loaded `team-protocol-local`, the line SHALL be `team-protocol loaded +local`.
Source: `src/ClaudeTeam/skills/team-protocol/SKILL.md` (section "Confirm that you loaded this skill"), `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`Protocol_gives_both_load_lines_and_the_local_skill_location`).

#### Scenario: Teammate with a local skill

- **WHEN** a repository has `team-protocol-local` and a teammate loads both skills
- **THEN** the shipped text tells the teammate to start its first message with `team-protocol loaded +local`

### Requirement: Leader check of the local load

The shipped `start-teammate` skill SHALL tell the leader to require `team-protocol loaded +local` in the first message of a teammate when the leader itself loaded `team-protocol-local`.
If the line is missing, the leader SHALL send one reminder, and then tell the operator. A missing line SHALL NOT count as a failed start.

#### Scenario: Missing local confirmation

- **WHEN** the leader loaded `team-protocol-local` and the first message of a teammate contains `team-protocol loaded` without `+local`
- **THEN** the shipped text tells the leader to send one reminder and, if the next message also lacks `+local`, to tell the operator

Source: `src/ClaudeTeam/skills/start-teammate/SKILL.md` (section "Confirm the protocol load"), `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`Start_skill_checks_the_local_load_of_a_teammate`). The reminder and the report to the operator are in `Start_skill_sends_one_reminder_and_then_tells_the_operator_when_the_local_line_is_missing`.

### Requirement: Leader load confirmation

The shipped team-leader agent SHALL tell the leader to start its first message to the operator with `team-leader loaded`, or with `team-leader loaded +local` when it loaded `team-leader-local`.
The operator checks this line, because no other party reads the load of the leader.
Source: `src/ClaudeTeam/team-leader.md`, `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`Agent_confirms_its_own_load_to_the_operator`).

#### Scenario: Leader with a local skill

- **WHEN** a repository has `team-leader-local` and the leader loads it
- **THEN** the shipped text tells the leader to start its first message to the operator with `team-leader loaded +local`

### Requirement: Leader uses the team skills

The shipped team-leader agent SHALL tell the leader to invoke `team-protocol` at startup, `start-teammate` just before each Agent call that starts a teammate, and `end-teammate` just before each shutdown request.
The agent SHALL NOT repeat the procedures of the skills.
Source: `src/ClaudeTeam/team-leader.md`, `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`Agent_uses_the_start_and_end_skills_and_repeats_no_procedure`, `Agent_body_invokes_team_protocol_after_the_local_line`).

#### Scenario: Start procedure

- **WHEN** the embedded team-leader agent is read
- **THEN** it names `start-teammate` and `end-teammate`
- **AND** it contains no `tmux list-panes` command

### Requirement: Team-leader agent front matter

The embedded team-leader agent SHALL start with front matter whose first field is `name: team-leader`.
The front matter SHALL set `description`, `effort: low`, `memory: local`, and an `initialPrompt` that names `team-protocol`, and SHALL NOT set `model:`.
Source: `src/ClaudeTeam/team-leader.md`, `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`Agent_front_matter_has_the_required_fields`).

#### Scenario: Agent front matter

- **WHEN** the embedded agent text is read
- **THEN** it starts with `---` followed by `name: team-leader`
- **AND** its front matter contains `effort: low` and an `initialPrompt` line that names `team-protocol`
- **AND** its front matter contains the line `memory: local`
- **AND** it contains no `model:` line

### Requirement: No repository conventions in shipped text

The shipped team files SHALL NOT contain `session-coordination`, `.scratch`, `.worktrees`, `dotnet`, `decisions.md`, `NOT RE-DERIVED`, `friction-scribe`, `preset`, `test-runner`, `implementer-`, `requirements-`, or `troubleshooter-`.
Source: `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`Shipped_text_has_no_repository_conventions`, one case for the agent and for each skill).

#### Scenario: Shipped skill text

- **WHEN** the embedded `start-teammate` skill is read
- **THEN** it contains none of the listed strings

### Requirement: Assignment is the source for reports and subagents

The shipped `team-protocol` skill SHALL tell a worker to take the report path, the subagent model, and the maximum subagent count from its assignment.
If the assignment names no count, the worker SHALL create no subagent. If it names no model, a subagent SHALL use the default model of the session. The assignment list SHALL keep the base commit.

#### Scenario: No subagent count

- **WHEN** the embedded `team-protocol` skill is read
- **THEN** it says that a worker whose assignment names no subagent count creates no subagent
- **AND** it names no model as a default

Source: `src/ClaudeTeam/skills/team-protocol/SKILL.md` (sections "The assignment", "Subagents", "Report"), `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`Protocol_takes_report_path_model_and_count_from_the_assignment`).

### Requirement: psmux and PowerShell forms

The shipped `start-teammate` and `end-teammate` skills SHALL give a psmux form next to each `tmux` command, and a PowerShell form next to each command that needs bash or `printenv`.
Each psmux form SHALL carry the words `not tested on Windows` until a run on Windows confirms it.

#### Scenario: Pane check

- **WHEN** the embedded `start-teammate` skill gives `tmux list-panes -a -F '#{pane_id} #{pane_title}'`
- **THEN** the same step gives the psmux form with the words `not tested on Windows`

#### Scenario: Wait loop

- **WHEN** the embedded `end-teammate` skill gives the bash wait loop
- **THEN** the same step gives a PowerShell form

Source: `src/ClaudeTeam/skills/start-teammate/SKILL.md`, `src/ClaudeTeam/skills/end-teammate/SKILL.md`, `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`Start_skill_gives_psmux_and_powershell_forms`, `End_skill_gives_psmux_and_powershell_forms`).

### Requirement: Checks decide a start or a shutdown

The shipped `start-teammate` and `end-teammate` skills SHALL say that psmux can behave differently from tmux and give no error.
They SHALL tell the leader to judge a start or a shutdown only by the verification checks, never by the exit code of a multiplexer command.

#### Scenario: Silent psmux difference

- **WHEN** the embedded `end-teammate` skill is read
- **THEN** it says that psmux can differ from tmux without an error
- **AND** it tells the leader to judge the shutdown by the checks

Source: `src/ClaudeTeam/skills/start-teammate/SKILL.md`, `src/ClaudeTeam/skills/end-teammate/SKILL.md`, `tests/ClaudeTeam.Tests/ShippedContentTests.cs` (`AssertChecksDecide`, called by `Start_skill_gives_psmux_and_powershell_forms` and `End_skill_gives_psmux_and_powershell_forms`).
