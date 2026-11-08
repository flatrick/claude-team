# A leader `rg` call with no path waited on standard input

- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: team leader session. A Bash call without a terminal. The same call also ran a worktree add.
- Observation: The leader ran `rg -n ... 'FRICTION.md|friction log|friction-log'` with no path argument.
  `rg` read standard input and waited.
  The Bash call hit its 120 s timeout and moved to the background.
  The leader stopped it with `TaskStop`.
  The leader reran with `.` as the path and `</dev/null`.
  The worktree add in the same call did not run.
  The call started at about 21:33 local time (UTC+1), from the `ps` start time. The `TaskStop` came after the 120 s timeout.
- Evidence: the command text is in the stopped task `b35tf1kte` in the leader session. NOT RE-DERIVED (second-hand — the leader saw it; the scribe did not).
  The documented behavior: `man rg | rg -n -i 'stdin'` (ripgrep 15.2.0) says: "ripgrep will automatically detect if stdin is a readable file and search stdin for a regex pattern, e.g. ls | rg foo. In some environments, stdin may exist when it shouldn't. To turn off stdin detection, one can explicitly specify the directory to search".
  `rg --help` does not state the rule in the lines that the scribe searched.
- Cause: verified for the rule in the man page. That the call had a readable stdin is inferred from the wait.
- Effect: About two minutes lost. The worktree add in the same call did not run.
- Status: open.
