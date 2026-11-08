# launcher/session/start Specification

## Purpose

Defines how `claude-team` starts a Claude team leader in a terminal multiplexer session: the backend it picks per platform, the `claude` command line, the agent-teams variable, the startup warnings, the dry run, and the handling of an existing session or a missing backend.

## Requirements

### Requirement: Multiplexer backend per platform

The tool SHALL use `psmux` as the multiplexer backend on Windows and `tmux` on every other platform.
Source: `src/ClaudeTeam/LaunchPlan.cs` (`BackendFor`), `src/ClaudeTeam/Program.cs` (lines 11-12), `tests/ClaudeTeam.Tests/LaunchPlanTests.cs`.

#### Scenario: Windows

- **WHEN** the tool runs on Windows
- **THEN** every launch command runs `psmux`

#### Scenario: Linux

- **WHEN** the tool runs on Linux
- **THEN** every launch command runs `tmux`

### Requirement: Claude command line

The tool SHALL start `claude` with `--teammate-mode tmux --name <session> --append-system-prompt claude-team-leader`, followed by the forwarded arguments.
The tool SHALL insert `--agent team-leader` directly after `claude` only when a team-leader agent is installed.
Source: `src/ClaudeTeam/LaunchPlan.cs` (`Build`), `src/ClaudeTeam/PeerRule.cs` (`Marker`), `tests/ClaudeTeam.Tests/LaunchPlanTests.cs` (`Without_the_agent_file_drops_only_the_agent_flag`, `Forwarded_arguments_go_last`).

#### Scenario: Agent installed

- **WHEN** a team-leader agent is installed and the session is `demo`
- **THEN** the command is `claude --agent team-leader --teammate-mode tmux --name demo --append-system-prompt claude-team-leader`

#### Scenario: Agent missing

- **WHEN** no team-leader agent is installed
- **THEN** the command omits only `--agent team-leader`

#### Scenario: Forwarded arguments

- **WHEN** the tool runs with `-- --model opus`
- **THEN** `--model opus` follows the system-prompt marker at the end of the command

### Requirement: Team-leader agent detection

The tool SHALL treat the team-leader agent as installed when `team-leader.md` exists in `.claude/agents/` under the project root or in `~/.claude/agents/`.
The project root SHALL be the git top-level folder, or the current folder outside git.
Source: `src/ClaudeTeam/Program.cs` (lines 38-39), `src/ClaudeTeam/LeaderAgent.cs` (`IsInstalled`), `tests/ClaudeTeam.Tests/LeaderAgentTests.cs`.

#### Scenario: Agent only in the user profile

- **WHEN** `~/.claude/agents/team-leader.md` exists and the repository has no agent file
- **THEN** the agent counts as installed

### Requirement: Missing-agent warning

When the team-leader agent is not installed, the tool SHALL write a warning to standard error that names `claude-team --install-leader-agent <repo|global|both>`, and SHALL continue the launch.
Source: `src/ClaudeTeam/Program.cs` (lines 40-42).

#### Scenario: Agent nowhere

- **WHEN** neither agent file exists
- **THEN** standard error shows a warning that names `--install-leader-agent`
- **AND** the launch continues without `--agent team-leader`

### Requirement: Peer-rule warning

The tool SHALL write a warning to standard error that names `claude-team --install-peer-rule <repo|global|both>` when `claude-team-peer-leaders.md` exists neither in `.claude/rules/` under the git top-level folder nor in `~/.claude/rules/`, and SHALL continue the launch.
Outside git, only `~/.claude/rules/` SHALL count.
Source: `src/ClaudeTeam/Program.cs` (lines 43-45), `src/ClaudeTeam/PeerRule.cs` (`IsInstalled`), `tests/ClaudeTeam.Tests/PeerRuleTests.cs`.

#### Scenario: Rule missing

- **WHEN** neither rule file exists
- **THEN** standard error shows a warning that names `--install-peer-rule`
- **AND** the launch continues

### Requirement: Session creation command

The tool SHALL create the session with `<backend> new-session [-d] -s <session> [-c <current folder>] -e CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1 <claude command>`.
The tool SHALL add `-d` only inside a pane and `-c` only on a platform other than Windows, as the next requirements state.
Source: `src/ClaudeTeam/LaunchPlan.cs` (`Build`), `tests/ClaudeTeam.Tests/LaunchPlanTests.cs` (`Outside_a_pane_on_linux_starts_an_attached_tmux_session_with_the_variable`).

#### Scenario: Linux outside a pane

- **WHEN** the tool runs on Linux outside a multiplexer pane in `/src/repo` with session `repo`
- **THEN** it runs one command: `tmux new-session -s repo -c /src/repo -e CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1` followed by the claude command

### Requirement: Session working directory

On a platform other than Windows, the tool SHALL add `-c <current folder>` after `-s <session>`.
On Windows, the tool SHALL omit `-c`, so the session inherits the working directory of the client.
Source: `src/ClaudeTeam/LaunchPlan.cs` (`Build`), `tests/ClaudeTeam.Tests/LaunchPlanTests.cs` (`On_windows_uses_psmux_and_omits_the_working_directory`).

#### Scenario: Windows

- **WHEN** the tool runs on Windows
- **THEN** the `psmux new-session` arguments contain no `-c`

### Requirement: Launch from inside a pane

The tool SHALL treat the launch as inside a pane when the `TMUX` environment variable is set and not blank.
Inside a pane, the tool SHALL add `-d` to `new-session` and SHALL then run `<backend> switch-client -t =<session>`.
Source: `src/ClaudeTeam/Program.cs` (line 47), `src/ClaudeTeam/LaunchPlan.cs` (`Build`), `tests/ClaudeTeam.Tests/LaunchPlanTests.cs` (`Inside_a_pane_creates_a_detached_session_and_switches_to_it`).

#### Scenario: Inside tmux

- **WHEN** `TMUX` is set and the session is `repo`
- **THEN** the tool runs `tmux new-session -d -s repo ...`
- **AND** then runs `tmux switch-client -t =repo`

### Requirement: Agent-teams variable

The tool SHALL set `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1` in the environment of each launch command, `new-session` and `switch-client`, in addition to the `-e` argument of `new-session`.
The `has-session` check SHALL run without that variable in its environment.
Source: `src/ClaudeTeam/Program.cs` (lines 69-76), `src/ClaudeTeam/Processes.cs` (`Run`, `Capture`), `src/ClaudeTeam/Multiplexer.cs` (`Check`).

#### Scenario: Launch commands get the variable

- **WHEN** the tool starts a session inside a tmux pane
- **THEN** `tmux new-session` and `tmux switch-client` run with `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1` in their environment
- **AND** `tmux has-session` runs without it

#### Scenario: Running tmux server

- **WHEN** a tmux server already runs
- **THEN** the new session still receives the variable through `-e`

### Requirement: Dry run of a launch

When `--dry-run` is given without an install option, the tool SHALL print `session: <session>`, then `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1`, then each launch command on its own line, SHALL start nothing, and SHALL exit with code 0.
Each argument that contains a space SHALL be printed in single quotes.
The dry run SHALL NOT check the backend or an existing session.
Source: `src/ClaudeTeam/Program.cs` (lines 50-57).

#### Scenario: Dry run without tmux installed

- **WHEN** the tool runs with `--dry-run` on a machine without tmux
- **THEN** it prints the session line, the variable line, and the commands
- **AND** the exit code is 0

### Requirement: Warnings before a dry run

The tool SHALL write the missing-agent and missing-peer-rule warnings to standard error before the dry-run output, because it checks for both files before it handles `--dry-run`.
Source: `src/ClaudeTeam/Program.cs` (lines 38-45 for the warnings, lines 50-57 for the dry run).

#### Scenario: Warnings before a dry run

- **WHEN** the tool runs with `--dry-run` and neither the team-leader agent nor the peer-leader rule is installed
- **THEN** both warnings are written to standard error before the dry-run output

### Requirement: Existing session

The tool SHALL check for the session with `<backend> has-session -t =<session>` before it starts anything.
When that command exits with code 0, the tool SHALL write a message to standard error that the session exists and that nothing was started, with commands to start under `<session>-2`, to attach, and to kill the session, and SHALL exit with code 1.
Source: `src/ClaudeTeam/Multiplexer.cs` (`Check`, `ExistsMessage`), `src/ClaudeTeam/Program.cs` (lines 59-67).

#### Scenario: Session already running

- **WHEN** a session with the resolved name exists
- **THEN** no command is run
- **AND** standard error shows `claude-team --session <session>-2`, `<backend> attach -t =<session>`, and `<backend> kill-session -t =<session>`
- **AND** the exit code is 1

### Requirement: Missing backend

The tool SHALL write `Cannot run '<backend>'. Install it and put it on PATH.` to standard error and exit with code 127 when the backend cannot be started.
Source: `src/ClaudeTeam/Multiplexer.cs` (`Check`), `src/ClaudeTeam/Processes.cs` (`NotFound`, `Capture`), `src/ClaudeTeam/Program.cs` (lines 61-63).

#### Scenario: tmux not on PATH

- **WHEN** the tool runs on Linux and `tmux` cannot be started
- **THEN** the exit code is 127

### Requirement: Run launch commands in order

The tool SHALL run the launch commands in order and SHALL stop at the first command that exits with a nonzero code, returning that code.
When a command cannot be started, the tool SHALL write `Cannot start '<file>'. Install it and put it on PATH.` with the system message to standard error and return 127.
The tool SHALL exit with code 0 when every command succeeds.
Source: `src/ClaudeTeam/Program.cs` (lines 71-77), `src/ClaudeTeam/Processes.cs` (`Run`).

#### Scenario: new-session fails

- **WHEN** `new-session` exits with code 1 inside a pane
- **THEN** `switch-client` is not run
- **AND** the exit code is 1
