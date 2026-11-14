## 1. Session name whitespace

- [x] 1.1 Red test: in `tests/ClaudeTeam.Tests/OptionsTests.cs`, add `--session "my repo"`, a tab, and a leading space to `Rejects_invalid_input` with the new message. Run it and confirm that it fails.
- [x] 1.2 Red test: in `tests/ClaudeTeam.Tests/SessionNameTests.cs`, add a test that `Sanitize("my repo")` is `my_repo`, and a test that `Resolve` replaces a space in the worktree part.
- [x] 1.3 Change `src/ClaudeTeam/Options.cs` and `src/ClaudeTeam/SessionName.cs` so tests 1.1 and 1.2 pass.
- [x] 1.4 Check the default-name text in `src/ClaudeTeam/Help.cs` and `README.md`. Update it if it lists the replaced characters.

## 2. Windows forwarded arguments

- [x] 2.1 Red test: in `tests/ClaudeTeam.Tests/LaunchPlanTests.cs`, add tests for `LaunchPlan.Validate`. Windows with `["--append-system-prompt", "a b"]` returns an error that names `a b`. Linux with the same input returns `null`. Windows with no space returns `null`. Run them and confirm that they fail (the method does not exist).
- [x] 2.2 Add `LaunchPlan.Validate` in `src/ClaudeTeam/LaunchPlan.cs`.
- [x] 2.3 Skipped: `src/ClaudeTeam/Program.cs` calls `OperatingSystem.IsWindows()` directly, so no process test can set `IsWindows` on Linux, and the change adds no seam. `LaunchPlan.Validate` tests cover the rule with `IsWindows: true`. A human checks the Windows launch end to end in task 4.4.
- [x] 2.4 Call `LaunchPlan.Validate` in `src/ClaudeTeam/Program.cs` before the launch output. Return 2 on an error.
- [x] 2.5 Fix the stale comment in `src/ClaudeTeam/LaunchPlan.cs` about quotes so that it names the new check.

## 3. tmux minimum version

- [x] 3.1 Red test: create `tests/ClaudeTeam.Tests/TmuxVersionTests.cs`. Cases that are supported: `tmux 3.2`, `tmux 3.2a`, `tmux 3.8`, `tmux 4.0`, `tmux 10.1`. Cases that are too old: `tmux 3.1c`, `tmux 3.0`, `tmux 2.9a`. Cases that continue: `tmux next-3.4`, `tmux master`, empty text. Run them and confirm that they fail.
- [x] 3.2 Add `TmuxVersion` and the minimum constant. Make 3.1 pass.
- [x] 3.3 Red test: add a process-level test with a fake `tmux` script on `PATH` that prints `tmux 3.1c` for `-V`, and succeeds for `has-session` with a nonzero exit code. Expect exit code 1 and the message on standard error. Skip and report if `ProgramProcessTests.cs` cannot support a fake `tmux`. A real tmux older than 3.2 cannot be tested on this host.
- [x] 3.4 Add `Multiplexer.CheckVersion` and call it in `src/ClaudeTeam/Program.cs` for a platform other than Windows. Do not call it for `--dry-run`.
- [x] 3.5 Document the minimum tmux version 3.2 in `README.md` and in `src/ClaudeTeam/Help.cs`. A change to a help string is a change to observable behavior; this change covers it.

## 4. Verify

- [x] 4.1 Run `dotnet test` (load the `dotnet-workflow` skill first) and confirm that every new test passes.
- [x] 4.2 Run `openspec validate multiplexer-launch-checks --strict`.
- [x] 4.3 Run `openspec validate --all --strict` and `openspec validate --archived`. Both must show no warnings.
- [x] 4.4 Report what a human must still check: `psmux` with a space on a Windows host, and a real tmux 3.1.
