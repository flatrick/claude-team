## Context

`LaunchPlan.Build` (`src/ClaudeTeam/LaunchPlan.cs`) builds the `new-session` command. On Windows it omits `-c` because `psmux` joins arguments without quotes. It still passes `-s <session>` and the forwarded arguments unquoted.
The default session name comes from `SessionName.Resolve` (`src/ClaudeTeam/SessionName.cs`), which takes a folder name or a repository name. `Sanitize` replaces only periods and colons. A folder `my repo` gives the session `my repo`.
`--session` is validated in `src/ClaudeTeam/Options.cs`. It rejects empty, control characters, periods, and colons.
`LaunchPlan.Build` always adds `-e CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1`. `Program.cs` never checks the tmux version.

## Facts and their sources

- tmux added `new-session -e` in 3.2. Source: tmux `CHANGES`, section "CHANGES FROM 3.1c TO 3.2": "Add -e flag for new-session to set environment variables, like the same flag for new-window." Re-derive: fetch `https://raw.githubusercontent.com/tmux/tmux/master/CHANGES` and search for `CHANGES FROM 3.1c TO 3.2`.
  Re-derived with: `rg -n 'CHANGES FROM 3.1c TO 3.2$|Add -e flag for new-session' CHANGES` on a full local copy (180199 bytes). The heading is on line 1238, and the entry is on lines 1450-1451 of that copy, before the next older heading at line 1613.
- The `psmux` argument join is NOT RE-DERIVED (second-hand, a code comment; no Windows host with `psmux`). Every Windows decision below treats it as true and avoids a dependency on exact `psmux` behavior.

## Decisions

The operator chose D1, D2, and D3 as written below.

### D1: Session name with whitespace

Replace whitespace with `_` in a default name, and reject whitespace in `--session`, on every platform.
Reason: one rule on every platform is simple to explain and to test on Linux. A user who types a name can fix it at once. A default name has no user error to report, so the tool repairs it, as it does for periods and colons.
Rejected: reject only on Windows (names differ by platform; a default name with a space would fail on Windows with no user error). Quote for `psmux` (the quote behavior is NOT RE-DERIVED and cannot be tested here).
Cost: `my repo` and `my_repo` give the same session name. A user who used `--session "my repo"` on Linux gets a new error.

### D2: Forwarded arguments with whitespace on Windows

Fail before launch with exit code 2 and a message that names the argument.
Reason: the tool cannot know how `psmux` joins arguments, so a quote can be wrong and break the launch silently. A clear failure is safer.
Rejected: quote the argument (unverifiable here). Do nothing and document (the silent split stays).
Cost: on Windows a user cannot forward an argument with a space until someone verifies `psmux` on a Windows host. The check does not apply to Linux, where tmux receives the arguments as separate values.

### D3: tmux minimum version

Check the version and fail with a clear message, and document the minimum in `README.md` and the help text.
Reason: the version is cheap to read, and the raw tmux error does not name the cause. Documentation alone leaves the failure in place.
Rejected: document only. A fallback with `set-environment` (a larger change; the tool already passes the variable in the process environment of each launch command, but a running tmux server needs `-e`; see `src/ClaudeTeam/Program.cs`, comment before `Processes.Run`).
Cost: one extra process call before a launch.

## Design

### Session name (Linux and Windows)

- `SessionName.Sanitize` also replaces each `char.IsWhiteSpace` character with `_`.
- `Options.Parse` rejects a `--session` value that is empty or has whitespace, a control character, a period, or a colon. It uses the message `Use a nonempty session name without whitespace, control characters, periods, or colons.`
- `Help.cs` text that describes the default name needs no change, except where it lists the replaced characters. Check this text during apply.

### Forwarded arguments (Windows only)

- Add `LaunchPlan.Validate(LaunchInputs inputs)`. It returns an error text or `null`. It returns an error only when `IsWindows` is true and a forwarded argument contains whitespace. It is a pure function, so a test can run it on Linux with `IsWindows: true`.
- `Program.cs` calls `LaunchPlan.Validate` after it builds the inputs and before `LaunchOutput.Write`. On an error it writes the text to standard error and returns 2.

### tmux version (non-Windows only)

- Add `TmuxVersion` with `TryParse(string text, out int major, out int minor)` and `IsSupported(text)`. The pattern is `tmux <major>.<minor>` with an optional suffix. Any other text is "unknown" and counts as supported.
- Add a `Multiplexer.CheckVersion` step that runs `tmux -V` through `Processes.Capture`. `Program.cs` calls it after the `Multiplexer.Check` switch and before the `Processes.Run` loop, only when `isWindows` is false. It is not part of the dry run, because the dry run never runs the backend.
- If the version is older than 3.2, write `tmux <found> is too old. claude-team needs tmux 3.2 or newer (new-session -e).` to standard error and return 1.
- The minimum is one constant. A test and the message use the same constant.

### Windows and non-Windows summary

| Check | Windows | Other |
|---|---|---|
| `--session` whitespace | rejected | rejected |
| default name whitespace | replaced | replaced |
| forwarded argument whitespace | rejected | allowed |
| tmux 3.2 check | not run | run before launch |

## Test limits on this Linux host

- Testable here: the `--session` rule, `Sanitize`, `LaunchPlan.Validate` with `IsWindows: true`, `TmuxVersion` parsing, and the process-level version check with a fake `tmux` script on `PATH` if `tests/ClaudeTeam.Tests/ProgramProcessTests.cs` supports it.
- Not testable here: real `psmux` behavior with a space, a real tmux older than 3.2, and a Windows launch end to end. Mark each as NOT RE-DERIVED and keep it open for a human with a Windows host or tmux 3.1.

## Writes outside the repository

None.

## Risks

- Whitespace in a repository folder name now changes the default session name. A user who attached to the old name with a space must use the new name.
- A tmux build with a version text that the pattern does not match skips the check, so it can still fail with the raw tmux error.
