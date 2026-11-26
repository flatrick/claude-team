---
name: end-teammate
description: Leader only. Invoke just before sending a shutdown_request to a teammate. Covers the request, the verification, and the expected rejected attempts.
---

Invoke the skill `end-teammate-local`. If the tool says `Unknown skill`, continue.

# End a teammate

psmux can behave differently from tmux and give no error. Judge a shutdown by the checks in steps 5 and 6, never by the exit code of a multiplexer command.

1. Shut a teammate down only when its assignment is done, or when the operator asks for it.
2. Call `SendMessage` with `to` set to the teammate name. Set `message` to a JSON OBJECT, not a string: `{"type": "shutdown_request", "reason": "<short reason>"}`.
3. The `team-protocol` skill, which you loaded at startup, gives the object form and the rejection text for a string. Use the object form.
4. If the first call is rejected, read the error. Do not send the same call again.
5. Verify the shutdown. The pane must be absent from the pane list. No entry in the `members` list of `~/.claude/teams/<team>/config.json` can have the name. Both changes happen within about 25 seconds after the request.
   - tmux: `tmux list-panes -a -F '#{pane_id} #{pane_title}'`
   - psmux: `psmux list-panes -a -F '#{pane_id} #{pane_title}'` (not tested on Windows)
6. Wait until no member entry in the config file has the name. Check only the member names. The `prompt` field of a different member can contain the name.
   - bash: `timeout 40 bash -c 'until python3 -I -c "import json,sys; sys.exit(sys.argv[2] in [m.get(\"name\") for m in json.load(open(sys.argv[1]))[\"members\"]])" "$0" "$1"; do sleep 1; done' ~/.claude/teams/<team>/config.json <name>`. Exit code 0 confirms that the name is gone. Any other exit code does not confirm it. A bare foreground `sleep` is blocked.
   - PowerShell (not tested on Windows): `$end = (Get-Date).AddSeconds(40); do { try { $names = (Get-Content "$HOME/.claude/teams/<team>/config.json" -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop).members.name; $gone = [bool]$names -and $names -notcontains '<name>' } catch { $gone = $false }; if (-not $gone) { Start-Sleep -Seconds 1 } } until ($gone -or (Get-Date) -ge $end); $gone`. The output `True` confirms that the name is gone. The output `False` does not confirm it.
7. A failed response from the teammate is not proof that the teammate survives. Always run the checks in steps 5 and 6.
8. Never use `TaskStop` first. Use it only if the teammate does not exit after the request. Capture the pane of the teammate first, as `start-teammate` says in "Start one agent at a time".

A teammate often sends the `shutdown_response` as a JSON string one or more times before the object form succeeds.
These rejected attempts are expected, and they are not an error.
The leader takes no action for them. The leader runs the checks in steps 5 and 6, and judges the shutdown only by those checks.
