---
name: end-teammate
description: Leader only. Invoke just before sending a shutdown_request to a teammate. Covers the request, the verification, and the expected rejected attempts.
---

# End a teammate

1. Shut a teammate down only after its work is complete and the operator asked for it, or after the assignment is done.
2. Call `SendMessage` with `to` set to the teammate name. Set `message` to a JSON OBJECT, not a string: `{"type": "shutdown_request", "reason": "<short reason>"}`.
3. The `team-protocol` skill, which you loaded at startup, gives the object form and the rejection text for a string. Use the object form.
4. If the first call is rejected, read the error. Do not send the same call again.
5. Verify the shutdown. The pane must be absent from `tmux list-panes -a -F '#{pane_id} #{pane_title}'`. The name must be absent from `~/.claude/teams/<team>/config.json`. Both changes happen within about 25 seconds after the request.
6. Wait with a `timeout` command and an `until` loop, for example `timeout 40 bash -c 'until ! grep -q "<name>" ~/.claude/teams/<team>/config.json; do sleep 1; done'`. A bare foreground `sleep` is blocked.
7. A failed response from the teammate is not proof that the teammate survives. Always run the checks in steps 5 and 6.
8. Never use `TaskStop` first. Use it only if the teammate does not exit after the request. Capture the pane of the teammate first, as `start-teammate` says in "Start one agent at a time".

A teammate often sends the `shutdown_response` as a JSON string one or more times before the object form succeeds.
These rejected attempts are expected, and they are not an error.
The leader takes no action for them. The leader runs the checks in steps 5 and 6, and judges the shutdown only by those checks.
