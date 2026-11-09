# launcher/starter-files/install Specification

## Purpose

This spec defines how `claude-team` installs its two starter files: the peer-leader rule and the team-leader agent.
It installs them into a repository, into the user profile, or into both. Both files use one install mechanism, so this spec covers them together.

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
Source: `src/ClaudeTeam/Installer.cs` (`Install`), `src/ClaudeTeam/PeerRule.cs`, `src/ClaudeTeam/LeaderAgent.cs`, `tests/ClaudeTeam.Tests/InstallerTests.cs`.

#### Scenario: Missing folders

- **WHEN** `.claude/rules/` does not exist under the target root
- **THEN** the tool creates it before it writes the file

### Requirement: Team skill files and locations

With the team-leader agent, the tool SHALL install the team skills `team-protocol`, `start-teammate`, and `end-teammate`, each as `SKILL.md` in `.claude/skills/<skill name>/`, to the same target as the agent.
Source: `src/ClaudeTeam/TeamSkills.cs` (`Names`, `Install`), `tests/ClaudeTeam.Tests/InstallerTests.cs`.

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
Source: `src/ClaudeTeam/Installer.cs` (`Run`), `src/ClaudeTeam/TeamSkills.cs` (`Names`), `tests/ClaudeTeam.Tests/InstallerTests.cs`.

#### Scenario: Both targets

- **WHEN** the tool runs with `--install-leader-agent both` in a git repository
- **THEN** it writes `<repo>/.claude/agents/team-leader.md` and `~/.claude/agents/team-leader.md`
- **AND** it writes the repository file and then the user profile file of each team skill
- **AND** it prints `Wrote <path>` for each file

### Requirement: Keep existing files unless forced

If `--force` is not given, the tool SHALL keep an existing target file and print `Kept the existing <path>. Use --force to replace it.`
If `--force` is given, the tool SHALL replace the file.
Source: `src/ClaudeTeam/Installer.cs` (`Write`, `Run`), `tests/ClaudeTeam.Tests/PeerRuleTests.cs` (`Install_keeps_an_existing_file_without_force`), `tests/ClaudeTeam.Tests/LeaderAgentTests.cs` (`Install_keeps_an_existing_file_without_force`).

#### Scenario: File exists without force

- **WHEN** `~/.claude/rules/claude-team-peer-leaders.md` exists and the tool runs with `--install-peer-rule global`
- **THEN** the file content is unchanged
- **AND** the exit code is 0

#### Scenario: File exists with force

- **WHEN** the same file exists and the tool runs with `--install-peer-rule global --force`
- **THEN** the file holds the embedded rule text

### Requirement: Repository target outside git

If the current folder is not in a git repository, the tool SHALL print `Cannot install .claude/<folder>/<file> to the repository: the current folder is not in a git repository.` for each repository target.
The tool SHALL then exit with code 1.
The tool SHALL still write the user profile file of a `both` target.
Source: `src/ClaudeTeam/Installer.cs` (`Install`, `Run`), `tests/ClaudeTeam.Tests/InstallerTests.cs` (`Outside_git_reports_the_path_and_exits_1`).

#### Scenario: Both outside git

- **WHEN** the tool runs with `--install-peer-rule both` outside git
- **THEN** it writes `~/.claude/rules/claude-team-peer-leaders.md`
- **AND** it reports that the repository file cannot be installed
- **AND** the exit code is 1

### Requirement: Dry run of an install

If `--dry-run` is given with an install option, the tool SHALL write no file.
The tool SHALL print `Would write <path>` or `Would keep the existing <path>. Use --force to replace it.` for each target.
The tool SHALL return the same exit code as a real run.
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

### Requirement: Repository copy of the peer rule

The repository's own `.claude/rules/claude-team-peer-leaders.md` SHALL be identical to the embedded source `src/ClaudeTeam/claude-team-peer-leaders.md`, and the test suite SHALL fail when they differ.
Source: `tests/ClaudeTeam.Tests/PeerRuleCopyTests.cs` (`Repository_rule_copy_matches_the_embedded_source`).

#### Scenario: Edited copy

- **WHEN** only one of the two files is edited
- **THEN** `Repository_rule_copy_matches_the_embedded_source` fails

### Requirement: Memory ignore reminder

The shipped agent sets `memory: local`, so Claude Code keeps the leader memory in `.claude/agent-memory-local/team-leader/`.
After the result lines of an install with `--install-leader-agent`, for every target and also in a dry run, the tool SHALL print one line that starts with `Reminder:` and tells the user to ignore `.claude/agent-memory-local/` in git.
The tool SHALL NOT edit any `.gitignore` file.

#### Scenario: Reminder after an agent install

- **WHEN** the tool runs with `--install-leader-agent repo` in a git repository
- **THEN** an output line after the result lines is `Reminder: add .claude/agent-memory-local/ to the .gitignore of each repository where you run the team leader, so that the leader memory is not committed.`
- **AND** the tool writes no `.gitignore` file and the exit code is 0
- **AND** the line is printed also with `--dry-run`

#### Scenario: Reminder before the note

- **WHEN** the tool runs with `--install-leader-agent both` in a git repository
- **THEN** the `Reminder:` line comes before the `Note:` line
- **AND** the `Note:` line is the last line of the output

#### Scenario: No reminder without the agent

- **WHEN** the tool runs with `--install-peer-rule repo` only, or with `--update repo`
- **THEN** no output line starts with `Reminder:`

Source: `src/ClaudeTeam/Installer.cs` (`MemoryReminder`, `Run`), `tests/ClaudeTeam.Tests/InstallerTests.cs` (`Dry_run_of_the_agent_install_prints_the_reminder_and_writes_nothing`, `Both_target_writes_the_repository_file_before_the_user_profile_file_and_ends_with_the_note`, `Peer_rule_alone_prints_no_reminder_and_no_note`, `Install_writes_no_gitignore_no_local_skill_and_no_other_agent`), `tests/ClaudeTeam.Tests/UpdaterTests.cs` (`Single_targets_print_no_note_and_no_reminder`).

### Requirement: Mixed precedence note for both targets

If `--install-leader-agent both` or `--update both` is given, the tool SHALL print one line after the result lines that says this: for a name in both places, Claude Code uses the user profile skill and the repository agent.
The line SHALL start with `Note:`, SHALL appear also in a dry run, and SHALL NOT change the exit code.

#### Scenario: Note after a both install

- **WHEN** the tool runs with `--install-leader-agent both` in a git repository
- **THEN** the last line of the output starts with `Note:` and names the user profile skill and the repository agent (a `Reminder:` line can come before it)
- **AND** the exit code is 0

#### Scenario: No note for one target

- **WHEN** the tool runs with `--install-leader-agent repo` or `--update global`
- **THEN** no output line starts with `Note:`

Source: `src/ClaudeTeam/Installer.cs` (`MixedPrecedenceNote`, `Run`), `src/ClaudeTeam/Updater.cs` (`Run`), `tests/ClaudeTeam.Tests/InstallerTests.cs` (`Both_target_writes_the_repository_file_before_the_user_profile_file_and_ends_with_the_note`, `Single_targets_print_no_note`), `tests/ClaudeTeam.Tests/UpdaterTests.cs` (`Both_handles_the_repository_file_before_the_user_profile_file_and_ends_with_the_note`, `Single_targets_print_no_note_and_no_reminder`).

### Requirement: Repository copies of the team-leader agent and the team skills

The repository's own `.claude/agents/team-leader.md` and `.claude/skills/<skill name>/SKILL.md` for each team skill SHALL be identical to their embedded sources, and the test suite SHALL fail when a copy differs.
The embedded sources are the single source of these files. This makes sure that the repository tests the text that users get, and that the two copies cannot drift apart.

#### Scenario: Edited skill copy

- **WHEN** only `.claude/skills/start-teammate/SKILL.md` is edited and its embedded source is not
- **THEN** the copy test fails

#### Scenario: Edited agent source

- **WHEN** only the embedded team-leader agent source is edited
- **THEN** the copy test fails

Source: `tests/ClaudeTeam.Tests/ShippedCopyTests.cs` (`Repository_copy_matches_the_embedded_source`, one case for each file of `ShippedFiles.All`).

### Requirement: Update mode

The tool SHALL run in update mode when `--update <target>` is given and help and version are not requested.
In update mode, the tool SHALL start no session, SHALL handle each updated file at each root of the target, and SHALL then exit.
The updated files are the peer-leader rule, the team-leader agent, and the team skills, in install order. For the target `both`, the repository file comes before the user profile file.

#### Scenario: Update instead of launch

- **WHEN** the tool runs with `--update global`
- **THEN** it updates the files under the user profile folder, prints the results, and exits without starting a session

Source: `src/ClaudeTeam/Updater.cs` (`Run`), `src/ClaudeTeam/ShippedFiles.cs` (`ShippedFiles.All`), `src/ClaudeTeam/Program.cs` (the update branch), `tests/ClaudeTeam.Tests/UpdaterTests.cs` (`Both_handles_the_repository_file_before_the_user_profile_file_and_ends_with_the_note`, `Mixed_states_add_replace_and_keep`), `tests/ClaudeTeam.Tests/ShippedHashesTests.cs` (`Shipped_files_are_the_rule_the_agent_and_the_skills_in_install_order`).

### Requirement: Update writes changed and missing files

For each updated file, the tool SHALL compare the file at the target with the embedded text, after it converts both to LF line endings.
If the file is missing, the tool SHALL write it and print `Added <path>`. If it differs, the tool SHALL replace it and print `Replaced <path>`. If it is the same, the tool SHALL leave it and print `Up to date <path>`.

#### Scenario: Mixed states

- **WHEN** the tool runs with `--update repo`, `team-protocol` is missing, `start-teammate` differs, and every other updated file is the same
- **THEN** it writes `team-protocol` and prints `Added <path>`
- **AND** it replaces `start-teammate` and prints `Replaced <path>`
- **AND** it leaves every other file unchanged and prints `Up to date <path>` for each

#### Scenario: Line endings only

- **WHEN** an installed file differs from the embedded text only in its line endings
- **THEN** the tool prints `Up to date <path>` and does not write the file

Source: `src/ClaudeTeam/Updater.cs` (`Handle`, `Write`), `tests/ClaudeTeam.Tests/UpdaterTests.cs` (`Mixed_states_add_replace_and_keep`, `A_file_that_differs_only_in_line_endings_counts_as_the_same`, `A_difference_only_in_the_final_newline_shows_a_diff_and_is_replaced`, `Written_files_have_lf_line_endings_only`).

### Requirement: Update writes only the updated files

The tool SHALL write no file other than the updated files during an update. It SHALL keep no record of an install or an update at the target.
Source: `src/ClaudeTeam/Updater.cs` (`Run`), `tests/ClaudeTeam.Tests/UpdaterTests.cs` (`An_update_writes_no_file_other_than_the_shipped_files`).

#### Scenario: No record file

- **WHEN** the tool runs with `--update repo`
- **THEN** every file that it writes is one of the updated files

### Requirement: Dry run of an update

If `--dry-run` is given with `--update`, the tool SHALL write no file.
For a missing file, it SHALL print `Would add <path>`. For a file that differs, it SHALL print `Would replace <path> (<label>)` and then a line diff. For a file that is the same, it SHALL print `Up to date <path>`.
The tool SHALL return the same exit code as a real run.
Source: `src/ClaudeTeam/Updater.cs` (`Handle`), `tests/ClaudeTeam.Tests/UpdaterTests.cs` (`Dry_run_writes_nothing_and_labels_each_change`).

#### Scenario: Dry run with a changed file

- **WHEN** the tool runs with `--update global --dry-run` and the user profile `team-protocol` skill differs from the embedded text
- **THEN** no file is written
- **AND** the output has the line `Would replace <path> (<label>)` followed by the line diff of that file

### Requirement: Line diff of an update

The line diff SHALL use the unified diff format with three lines of context.
Its header SHALL be `--- <installed path>` and `+++ <file name> (shipped)`. A line that starts with `-` is in the installed file only. A line that starts with `+` is in the embedded text only.
The tool SHALL make the diff itself, without an external program.
If a text has no final newline, the line `\ No newline at end of file` SHALL follow the last line of that text.

#### Scenario: One changed line

- **WHEN** an installed skill differs from the embedded text in one line
- **THEN** the diff has one hunk with that line as a `-` line and a `+` line, and up to three unchanged lines before and after it

#### Scenario: Difference only in the final newline

- **WHEN** the installed file is `a\nb` and the embedded text is `a\nb\n`
- **THEN** the diff has the unchanged line `a`, then the `-b` line, then the line `\ No newline at end of file`, then the `+b` line
- **AND** if the installed file is `a\nb\n` and the embedded text is `a\nb`, the marker line follows the `+b` line

Source: `src/ClaudeTeam/LineDiff.cs` (`Unified`), `src/ClaudeTeam/Updater.cs` (`Handle`), `tests/ClaudeTeam.Tests/LineDiffTests.cs` (`One_changed_line_gives_one_hunk_with_three_lines_of_context`, `Context_stops_at_the_start_and_end_of_the_file`, `Added_and_removed_lines_are_shown`, `Far_apart_changes_give_two_hunks`, `Line_endings_do_not_count_as_a_difference`, `A_missing_final_newline_is_a_visible_difference`, `An_extra_final_newline_is_a_visible_difference`, `Same_text_without_a_final_newline_gives_no_hunk`).

### Requirement: Shipped-version hashes

The tool SHALL embed a list of SHA-256 hashes, one for each version of each updated file that the tool shipped, the current version included. The hash is over the UTF-8 text with LF line endings.
In a dry run, the label of a file that differs SHALL be `older shipped version` if its hash is in the list for that file, and `edited` if it is not.

#### Scenario: Older shipped version

- **WHEN** an installed `end-teammate` skill holds the text of an earlier shipped version
- **THEN** the dry run prints `Would replace <path> (older shipped version)`

#### Scenario: Edited file

- **WHEN** a user edited the `effort:` line of an installed `team-leader.md`
- **THEN** the dry run prints `Would replace <path> (edited)`

Source: `src/ClaudeTeam/ShippedFiles.cs` (`ShippedHashes`), `src/ClaudeTeam/shipped-hashes.txt`, `src/ClaudeTeam/Updater.cs` (`Handle`), `tests/ClaudeTeam.Tests/ShippedHashesTests.cs` (`A_hash_belongs_to_one_published_path`, `Line_endings_do_not_change_the_hash`), `tests/ClaudeTeam.Tests/UpdaterTests.cs` (`An_earlier_shipped_text_is_labeled_older_shipped_version`, `Dry_run_writes_nothing_and_labels_each_change`).

### Requirement: Hash list includes the current text

The test suite SHALL fail when the hash list lacks the hash of the current embedded text of an updated file.
A change to a shipped file must add the hash of its new text in the same change. Because of this, each released text is in the list without a release step.
Source: `tests/ClaudeTeam.Tests/ShippedHashesTests.cs` (`Every_shipped_file_has_the_hash_of_its_current_text_in_the_list`), `src/ClaudeTeam/shipped-hashes.txt`.

#### Scenario: Edited shipped source without a new hash

- **WHEN** a shipped skill source is edited and the hash list is not changed
- **THEN** the hash test fails

### Requirement: Update outside git

If the current folder is not in a git repository, the tool SHALL print `Cannot update <path> in the repository: the current folder is not in a git repository.` for each updated file of a repository target, and SHALL exit with code 1.
The tool SHALL still update the user profile files of a `both` target.

#### Scenario: Update both outside git

- **WHEN** the tool runs with `--update both` outside git
- **THEN** it updates the files under the user profile folder
- **AND** it prints `Cannot update <path> in the repository: the current folder is not in a git repository.` for each repository file
- **AND** the exit code is 1

This line differs on purpose from the install line, `Cannot install <path> to the repository: ...`. The verb must match the command that the user ran. `<path>` is the path of the file under `.claude/`, relative to the repository root.

Source: `src/ClaudeTeam/Updater.cs` (`Run`), `tests/ClaudeTeam.Tests/UpdaterTests.cs` (`Outside_git_reports_each_repository_file_updates_the_profile_and_exits_1`).
