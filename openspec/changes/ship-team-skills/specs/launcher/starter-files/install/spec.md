## MODIFIED Requirements

### Requirement: Starter files and locations

The tool SHALL install the peer-leader rule as `claude-team-peer-leaders.md` in `.claude/rules/` and the team-leader agent as `team-leader.md` in `.claude/agents/`.
With the team-leader agent, the tool SHALL install the team skills `team-protocol`, `start-teammate`, and `end-teammate`, each as `SKILL.md` in `.claude/skills/<skill name>/`, to the same target as the agent.
The target `repo` SHALL write under the git top-level folder, `global` SHALL write under the user profile folder, and `both` SHALL write to both.
Source: `src/ClaudeTeam/Installer.cs` (`Install`), `src/ClaudeTeam/PeerRule.cs`, `src/ClaudeTeam/LeaderAgent.cs`. The team skills have no code yet.

#### Scenario: Missing folders

- **WHEN** `.claude/rules/` does not exist under the target root
- **THEN** the tool creates it before it writes the file

#### Scenario: Agent and skills together

- **WHEN** the tool runs with `--install-leader-agent repo` in a git repository
- **THEN** it writes `<repo>/.claude/agents/team-leader.md`
- **AND** it writes `<repo>/.claude/skills/team-protocol/SKILL.md`, `<repo>/.claude/skills/start-teammate/SKILL.md`, and `<repo>/.claude/skills/end-teammate/SKILL.md`

#### Scenario: No local skill is installed

- **WHEN** the tool runs with any install option and any target
- **THEN** it writes no skill whose name ends with `-local`
- **AND** it writes no agent definition other than `team-leader.md`

### Requirement: Install order

For the target `both`, the tool SHALL write the repository file before the user profile file.
If both install options are given, the tool SHALL install the peer-leader rule before the team-leader agent.
The tool SHALL install the team-leader agent before the team skills, and the team skills in the order `team-protocol`, `start-teammate`, `end-teammate`.
Source: `src/ClaudeTeam/Installer.cs` (`Install`, `Run`), `tests/ClaudeTeam.Tests/InstallerTests.cs` (`Combined_run_writes_rule_and_agent`). The order of the team skills has no code yet.

#### Scenario: Both targets

- **WHEN** the tool runs with `--install-leader-agent both` in a git repository
- **THEN** it writes `<repo>/.claude/agents/team-leader.md` and `~/.claude/agents/team-leader.md`
- **AND** it writes the repository file and then the user profile file of each team skill
- **AND** it prints `Wrote <path>` for each file

## ADDED Requirements

### Requirement: Mixed precedence note for both targets

If `--install-leader-agent both` is given, the tool SHALL print one line after the result lines that says this: for a name in both places, Claude Code uses the user profile skill and the repository agent.
The line SHALL start with `Note:`, SHALL appear also in a dry run, and SHALL NOT change the exit code.
Source: no code yet.

#### Scenario: Note after a both install

- **WHEN** the tool runs with `--install-leader-agent both` in a git repository
- **THEN** the last line of the output starts with `Note:` and names the user profile skill and the repository agent
- **AND** the exit code is 0

#### Scenario: No note for one target

- **WHEN** the tool runs with `--install-leader-agent repo` or `--install-leader-agent global`
- **THEN** no output line starts with `Note:`

### Requirement: Repository copies of the team-leader agent and the team skills

The repository's own `.claude/agents/team-leader.md` and `.claude/skills/<skill name>/SKILL.md` for each team skill SHALL be identical to their embedded sources, and the test suite SHALL fail when a copy differs.
The embedded sources are the single source of these files. This makes sure that the repository tests the text that users get, and that the two copies cannot drift apart.
Source: no code yet.

#### Scenario: Edited skill copy

- **WHEN** only `.claude/skills/start-teammate/SKILL.md` is edited and its embedded source is not
- **THEN** the copy test fails

#### Scenario: Edited agent source

- **WHEN** only the embedded team-leader agent source is edited
- **THEN** the copy test fails

## REMOVED Requirements

### Requirement: Team-leader agent content

**Reason**: The content of every shipped starter file, the team-leader agent included, now has its own spec.
**Migration**: See `launcher/starter-files/content`, requirement "Team-leader agent front matter" and requirement "No repository conventions in shipped text".
