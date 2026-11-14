## ADDED Requirements

### Requirement: Windows argument whitespace check

On Windows, the tool SHALL write a message that names the first forwarded argument with whitespace to standard error, and exit with code 2, before it runs a launch command.
The check SHALL run before the dry-run output. The tool SHALL NOT apply it on another platform.

#### Scenario: Windows forwarded argument with a space

- **WHEN** the tool runs on Windows with `-- --append-system-prompt "a b"`
- **THEN** standard error names the argument `a b`
- **AND** no command runs
- **AND** the exit code is 2

#### Scenario: Linux forwarded argument with a space

- **WHEN** the tool runs on Linux with `-- --append-system-prompt "a b"`
- **THEN** the check passes and the launch continues

#### Scenario: Basis of the Windows rule

- **WHEN** a reader asks why the tool rejects the argument
- **THEN** the reason is that `psmux` joins arguments without quotes
- **AND** this fact is NOT RE-DERIVED (second-hand, a code comment in `src/ClaudeTeam/LaunchPlan.cs`; no Windows host with `psmux` was available)

Source: `src/ClaudeTeam/LaunchPlan.cs` (`Validate`), `src/ClaudeTeam/Program.cs` (the `LaunchPlan.Validate` call before the launch output), `tests/ClaudeTeam.Tests/LaunchPlanTests.cs` (`Validate_on_windows_rejects_a_forwarded_argument_with_a_space_and_names_it`, `Validate_on_linux_accepts_a_forwarded_argument_with_a_space`, `Validate_on_windows_accepts_forwarded_arguments_without_whitespace`, `Validate_on_windows_names_the_first_argument_with_a_tab`).

### Requirement: Minimum tmux version

On a platform other than Windows, the tool SHALL run `tmux -V` after the backend and session checks and before the first launch command.
If the version is older than 3.2, the tool SHALL write a message that names the found version and the minimum to standard error, start nothing, and exit with code 1.
The dry run SHALL NOT run `tmux -V`.

#### Scenario: Old tmux

- **WHEN** the tool runs on Linux and `tmux -V` prints `tmux 3.1c`
- **THEN** standard error names version 3.1c and the minimum 3.2
- **AND** no `new-session` command runs
- **AND** the exit code is 1

#### Scenario: Supported tmux

- **WHEN** `tmux -V` prints `tmux 3.2a`, `tmux 3.8`, or `tmux 4.0`
- **THEN** the launch continues

#### Scenario: Dry run

- **WHEN** the tool runs with `--dry-run`
- **THEN** the tool does not run `tmux -V`

#### Scenario: Basis of the minimum

- **WHEN** a reader asks why the minimum is 3.2
- **THEN** the reason is that tmux added `new-session -e` in 3.2
- **AND** the source is the tmux `CHANGES` file, section "CHANGES FROM 3.1c TO 3.2": "Add -e flag for new-session to set environment variables, like the same flag for new-window."

Source: `src/ClaudeTeam/TmuxVersion.cs` (`MinimumMajor`, `MinimumMinor`, `IsSupported`, `TooOldMessage`), `src/ClaudeTeam/Multiplexer.cs` (`CheckVersion`), `src/ClaudeTeam/Program.cs` (the `Multiplexer.CheckVersion` call before the launch commands), `tests/ClaudeTeam.Tests/TmuxVersionTests.cs` (`Supported_versions_pass`, `Old_versions_fail`, `Message_names_the_found_version_and_the_minimum`), `tests/ClaudeTeam.Tests/ProgramProcessTests.cs` (`Old_tmux_stops_the_launch_with_exit_1_and_names_the_version`).

### Requirement: Unknown tmux version text

If the output of `tmux -V` is not `tmux <major>.<minor>` with an optional suffix, or if `tmux -V` fails, the tool SHALL continue the launch without a version error.
Examples of such output are `tmux next-3.4` and `tmux master`.
The tool SHALL NOT check the version on Windows.

#### Scenario: Development build

- **WHEN** `tmux -V` prints `tmux next-3.4` or `tmux master`
- **THEN** the launch continues

#### Scenario: Version command fails

- **WHEN** `tmux -V` exits with a nonzero code
- **THEN** the launch continues

Source: `src/ClaudeTeam/TmuxVersion.cs` (`TryParse`, `IsSupported`), `src/ClaudeTeam/Multiplexer.cs` (`CheckVersion`), `tests/ClaudeTeam.Tests/TmuxVersionTests.cs` (`Unknown_text_continues`, `TryParse_returns_major_and_minor`), `tests/ClaudeTeam.Tests/ProgramProcessTests.cs` (`Unknown_version_text_continues_to_new_session`, `Failed_version_command_continues_to_new_session`).
