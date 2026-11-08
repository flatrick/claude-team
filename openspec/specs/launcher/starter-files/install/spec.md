# launcher/starter-files/install Specification

## Purpose

Defines how `claude-team` installs its two starter files, the peer-leader rule and the team-leader agent, into a repository, into the user profile, or both. Both files use one install mechanism, so this capability covers them together.

## Requirements

### Requirement: Install mode

The tool SHALL run in install mode when `--install-peer-rule` or `--install-leader-agent` is given and help is not requested.
In install mode, the tool SHALL start no session, SHALL print one line per target file, and SHALL then exit.
Source: `src/ClaudeTeam/Program.cs` (lines 24-36).

#### Scenario: Install instead of launch

- **WHEN** the tool runs with `--install-peer-rule global`
- **THEN** it installs the rule, prints the result, and exits without starting a session

### Requirement: Starter files and locations

The tool SHALL install the peer-leader rule as `claude-team-peer-leaders.md` in `.claude/rules/` and the team-leader agent as `team-leader.md` in `.claude/agents/`.
The target `repo` SHALL write under the git top-level folder, `global` SHALL write under the user profile folder, and `both` SHALL write to both.
Source: `src/ClaudeTeam/Installer.cs` (`Install`), `src/ClaudeTeam/PeerRule.cs`, `src/ClaudeTeam/LeaderAgent.cs`.

#### Scenario: Missing folders

- **WHEN** `.claude/rules/` does not exist under the target root
- **THEN** the tool creates it before it writes the file

### Requirement: Install order

The tool SHALL write the repository file before the user profile file for the target `both`, and SHALL install the peer-leader rule before the team-leader agent when both install options are given.
Source: `src/ClaudeTeam/Installer.cs` (`Install`, `Run`), `tests/ClaudeTeam.Tests/InstallerTests.cs` (`Combined_run_writes_rule_and_agent`).

#### Scenario: Both targets

- **WHEN** the tool runs with `--install-leader-agent both` in a git repository
- **THEN** it writes `<repo>/.claude/agents/team-leader.md` and `~/.claude/agents/team-leader.md`
- **AND** it prints `Wrote <path>` for each file

### Requirement: Keep existing files unless forced

The tool SHALL keep an existing target file and print `Kept the existing <path>. Use --force to replace it.` when `--force` is not given.
With `--force`, the tool SHALL replace the file.
Source: `src/ClaudeTeam/Installer.cs` (`Write`, `Run`), `tests/ClaudeTeam.Tests/PeerRuleTests.cs` (`Install_keeps_an_existing_file_without_force`), `tests/ClaudeTeam.Tests/LeaderAgentTests.cs` (`Install_keeps_an_existing_file_without_force`).

#### Scenario: File exists without force

- **WHEN** `~/.claude/rules/claude-team-peer-leaders.md` exists and the tool runs with `--install-peer-rule global`
- **THEN** the file content is unchanged
- **AND** the exit code is 0

#### Scenario: File exists with force

- **WHEN** the same file exists and the tool runs with `--install-peer-rule global --force`
- **THEN** the file holds the embedded rule text

### Requirement: Repository target outside git

The tool SHALL print `Cannot install .claude/<folder>/<file> to the repository: the current folder is not in a git repository.` for each repository target when the current folder is not in a git repository, and SHALL exit with code 1.
The tool SHALL still write the user profile file of a `both` target.
Source: `src/ClaudeTeam/Installer.cs` (`Install`, `Run`), `tests/ClaudeTeam.Tests/InstallerTests.cs` (`Outside_git_reports_the_path_and_exits_1`).

#### Scenario: Both outside git

- **WHEN** the tool runs with `--install-peer-rule both` outside git
- **THEN** it writes `~/.claude/rules/claude-team-peer-leaders.md`
- **AND** it reports that the repository file cannot be installed
- **AND** the exit code is 1

### Requirement: Dry run of an install

When `--dry-run` is given with an install option, the tool SHALL write no file, SHALL print `Would write <path>` or `Would keep the existing <path>. Use --force to replace it.` for each target, and SHALL return the same exit code as a real run.
Source: `src/ClaudeTeam/Installer.cs` (`Run`, `Write`), `tests/ClaudeTeam.Tests/InstallerTests.cs` (`Dry_run_writes_nothing_and_reports_what_would_happen`, `Dry_run_outside_git_still_exits_1`).

#### Scenario: Dry run outside git

- **WHEN** the tool runs with `--install-peer-rule repo --dry-run` outside git
- **THEN** no file is written
- **AND** the exit code is 1

### Requirement: Embedded file content

The tool SHALL write the text of the starter files from resources embedded in the tool assembly, with LF line endings only.
The embedded peer-leader rule SHALL contain the marker `claude-team-leader`.
Source: `src/ClaudeTeam/ClaudeTeam.csproj` (`EmbeddedResource`), `src/ClaudeTeam/Installer.cs` (`ReadEmbedded`), `tests/ClaudeTeam.Tests/PeerRuleTests.cs` (`Embedded_text_contains_the_marker`), `tests/ClaudeTeam.Tests/LeaderAgentTests.cs` (`Written_file_has_lf_line_endings_only`).

#### Scenario: Written agent file

- **WHEN** the tool installs the team-leader agent
- **THEN** the file contains no carriage return

### Requirement: Team-leader agent content

The embedded team-leader agent SHALL start with front matter whose first field is `name: team-leader`, SHALL NOT set `model:`, and SHALL NOT refer to this repository's coordination file, `.scratch`, `.worktrees`, or its preset names.
Source: `tests/ClaudeTeam.Tests/LeaderAgentTests.cs` (`Embedded_text_starts_with_frontmatter_naming_the_agent`, `Embedded_text_does_not_refer_to_this_repository`), `src/ClaudeTeam/team-leader.md`.

#### Scenario: Agent front matter

- **WHEN** the embedded agent text is read
- **THEN** it starts with `---` followed by `name: team-leader`
- **AND** it contains no `model:` line

### Requirement: Repository copy of the peer rule

The repository's own `.claude/rules/claude-team-peer-leaders.md` SHALL be identical to the embedded source `src/ClaudeTeam/claude-team-peer-leaders.md`, and the test suite SHALL fail when they differ.
Source: `tests/ClaudeTeam.Tests/PeerRuleCopyTests.cs` (`Repository_rule_copy_matches_the_embedded_source`).

#### Scenario: Edited copy

- **WHEN** only one of the two files is edited
- **THEN** `Repository_rule_copy_matches_the_embedded_source` fails
