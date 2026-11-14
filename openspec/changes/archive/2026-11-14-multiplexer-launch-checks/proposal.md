## Why

On Windows, `psmux` joins arguments without quotes. A session name or a forwarded argument with a space then splits into several arguments, and the launch fails or starts the wrong command. A default session name comes from a folder name, so a folder `my repo` triggers this with no user error.
On Linux and macOS, the launcher always passes `new-session -e`. tmux supports this flag only from version 3.2. An older tmux prints a raw tmux error, and nothing documents the minimum version.

## What Changes

- Reject a `--session` value that contains whitespace, on every platform.
- Replace each whitespace character with an underscore in a default session name, as the tool does today for periods and colons.
- On Windows, stop before launch with a clear message when a forwarded argument contains whitespace. The check also applies to `--dry-run`.
- On a platform other than Windows, run `tmux -V` before the launch. If the version is older than 3.2, print a clear message and exit with code 1. If the version text does not parse, continue.
- Document the minimum tmux version in `README.md` and in the help text.
- **BREAKING** (small): `--session "my repo"` was accepted before and is now rejected. The default name for `my repo` changes from `my repo` to `my_repo`.

## Capabilities

### Modified Capabilities

- `launcher/command-line`: requirement "Session name validation" adds whitespace.
- `launcher/session/naming`: requirement "Sanitize default names" adds whitespace.

### Added Capabilities

- `launcher/session/start`: new requirements "Windows argument whitespace check" and "Minimum tmux version".

## Spec paths touched

- `openspec/specs/launcher/command-line/spec.md`
- `openspec/specs/launcher/session/naming/spec.md`
- `openspec/specs/launcher/session/start/spec.md`

## Out of scope

- Quoting arguments for `psmux`. The `psmux` join behavior is NOT RE-DERIVED (second-hand, a code comment in `src/ClaudeTeam/LaunchPlan.cs`; no Windows host with `psmux` was available). The change rejects the input instead of quoting it.
- A fallback to `set-environment` for tmux older than 3.2.
- A version check for `psmux`.
- The finding `code-session-flag-swallows-option` (`--session --dry-run`) and all other findings of the review.
- Any edit of the `Source:` lines in `openspec/specs/launcher/**` that the worktree `docs/review-fixes` owns.

## Impact

- Code: `src/ClaudeTeam/Options.cs`, `src/ClaudeTeam/SessionName.cs`, `src/ClaudeTeam/LaunchPlan.cs`, `src/ClaudeTeam/Multiplexer.cs`, `src/ClaudeTeam/Program.cs`, `src/ClaudeTeam/Help.cs`.
- Tests: `tests/ClaudeTeam.Tests/OptionsTests.cs`, `SessionNameTests.cs`, `LaunchPlanTests.cs`, and a new test file for the tmux version.
- Docs: `README.md`.
- The change adds no write outside the repository.
