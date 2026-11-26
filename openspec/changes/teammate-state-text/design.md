## Context

See `proposal.md` ("Why"). These constraints apply:

- A change to a shipped file needs the hash of its new text in `src/ClaudeTeam/shipped-hashes.txt`. The list holds each shipped version of each updated file. `--update --dry-run` uses it to label an installed file `older shipped version` or `edited` (`ShippedHashes` in `src/ClaudeTeam/ShippedFiles.cs`). `ShippedHashesTests` fails when the current hash is missing.
- The files under `.claude/skills/` are copies of the shipped sources. `ShippedCopyTests` fails when a copy differs.
- `ShippedContentTests.Shipped_text_has_no_repository_conventions` forbids some repository terms in shipped text. For this reason, evidence labels are in this design and not in the shipped text.

## Goals / Non-Goals

**Goals:**

- The shutdown check gives no wrong result for a name in a `prompt` field.
- A read error never confirms a shutdown.
- The leader and each teammate load the message state guidance before they need it.

**Non-Goals:**

- A tool that reads the team files. See `proposal.md` ("Out of scope").
- A claim that a message that left the inbox file was read.

## Decisions

### D1: Fix the shipped text only

The change adds no command to `claude-team`. The operator decided this.
Reason: most causes are in the harness, and the files under `~/.claude/teams/` are not a stable interface. On 2026-11-07, inbox files held messages with `read: false`. Today a delivered message leaves the inbox, so the inbox files are `[]` after delivery.

- The 2026-11-07 observation: NOT RE-DERIVED (conditions gone — the team folder of that run does not exist). The source is `friction/2026-11-07-sendmessage-treated-as-delivery.md`.
- The current observation: on 2026-11-26, with Claude Code 2.1.332, each file under `inboxes/` of one live team held an empty list. Re-derive in a live team: `cat ~/.claude/teams/<team>/inboxes/*.json`. This shows the state of the files, not that each message was delivered.

### D2: Check member names with the standard JSON parser

The operator chose this check. The bash form uses `python3 -I -c` with the `json` module. The PowerShell form uses `ConvertFrom-Json`.
Reason: a search of the whole config matches a teammate name in the `prompt` field of a different member. A JSON parser can read only the member names.

- Use `python3`, not `python`. The operator named `python3`.
- `-I` starts Python in isolated mode. Then a `json.py` file in the current folder cannot replace the standard module.
- Rejected: `jq`. The operator rejected it, because several tools have the name `jq` and their syntax differs.
- Rejected: a `grep` pattern for the `name` field. It depends on the layout of the JSON text, and a `prompt` field can contain the same text.

### D3: The exit code of Python says "still present"

The Python program ends with exit code 1 when a member has the name, and 0 when no member has it. The loop is `until <check>; do sleep 1; done`.
Reason: Python also ends with a nonzero code when it cannot read or parse the file. A missing `python3` gives a nonzero code too. With this direction, each error keeps the loop in the wait state, and `timeout` then ends with code 124.

Rejected: exit code 0 when a member has the name, with a `while <check>` loop. An error then ends the loop with code 0, which reads as a confirmed shutdown.
Measured on 2026-11-26 with that rejected form: a config with incomplete JSON and a missing config each ended the loop with code 0.

### D4: The text of step 5 and step 6

Step 5 keeps its two conditions. Replace its second condition with: "No entry in the `members` list of `~/.claude/teams/<team>/config.json` can have the name."

Step 6 has this text. The two commands are exact.

- Wait until no member entry in the config file has the name. Check only the member names. The `prompt` field of a different member can contain the name.
- bash:

  ```bash
  timeout 40 bash -c 'until python3 -I -c "import json,sys; sys.exit(sys.argv[2] in [m.get(\"name\") for m in json.load(open(sys.argv[1]))[\"members\"]])" "$0" "$1"; do sleep 1; done' ~/.claude/teams/<team>/config.json <name>
  ```

  Exit code 0 confirms that the name is gone. Any other exit code does not confirm it. A bare foreground `sleep` is blocked.
- PowerShell (not tested on Windows):

  ```powershell
  $end = (Get-Date).AddSeconds(40); do { try { $names = (Get-Content "$HOME/.claude/teams/<team>/config.json" -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop).members.name; $gone = [bool]$names -and $names -notcontains '<name>' } catch { $gone = $false }; if (-not $gone) { Start-Sleep -Seconds 1 } } until ($gone -or (Get-Date) -ge $end); $gone
  ```

  The output `True` confirms that the name is gone. The output `False` does not confirm it.

The implementer can use the layout of the current skill, with each command in one inline code span or in a code block. The command text stays on one line.

- `sys.exit` gets a boolean. Python uses `True` as exit code 1 and `False` as exit code 0.
- The path and the name are arguments after the script of `bash -c`. The shell then expands `~`, and the name is not part of the Python source.
- The bash form does not hide the error output. A leader can then see the cause of a timeout, for example a missing `python3`.
- `[bool]$names` rejects an empty config file and a config without member names. A live team always has the entry of the leader.

Measurements on 2026-11-26, on Linux, with fixture files. The bash form used `timeout 3` in place of `timeout 40`:

| Config fixture | bash exit code | PowerShell output |
|---|---|---|
| name only in a `prompt` field | 0 | `True` |
| name in a member entry | 124 | `False` |
| one member only, which has the name | 124 | `False` |
| incomplete JSON | 124 | `False` |
| empty file | 124 | `False` |
| missing file | 124 | `False` |

The earlier `grep` loop ended with code 124 for the first fixture. This is the false positive of the friction entry.
Task 1.5 gives the steps that create the fixtures again.
The PowerShell run used `pwsh` on Linux, with the same expressions, a 3 second limit, and a fixture path. It is not a run on Windows, so the label `not tested on Windows` stays.

### D5: One home for the message state guidance

The guidance is one new section, `## Message state`, in the shipped `team-protocol` skill. Put it after the section "The assignment".
Reason: each role that sends messages needs the fact about a send, and this skill applies to each of them. The leader and each teammate load it at startup, before the first message.

- Rejected: the team-leader agent. The harness delivers it only to the leader, so a teammate gets no text about a send.
- Rejected: the agent for the idle notice, and the skill for the other facts. This splits one topic into two files. Only a leader needs the idle notice text, so a teammate loads some text that it can fail to need. One section is worth this cost.
- No other shipped file repeats this text.

The section has this text:

> A successful `SendMessage` result means that the message reached the inbox of the recipient.
> It does not mean that the recipient read the message.
> The inbox file of a member is `~/.claude/teams/<team>/inboxes/<name>.json`.
> A message that is still in this file is not yet read.
> If an idle notice arrives after you sent a newer message, read the inbox file of the teammate.
> If the file is empty or holds only `[]`, the teammate has the message.
> Wait for its reply, and do not treat the notice as a refusal.

The operator chose the inbox check for a late idle notice.
Reason: no shipped step records the send time, and the `SendMessage` result shows no time. The same section already names the inbox file.

- Rejected: compare the time of the notice with the time of the last message. The reader has no reliable send time.
- The `SendMessage` result without a time field: NOT RE-DERIVED (conditions gone — the result seen on 2026-11-26 exists only in that session).

The operator decided that a file that is empty and a file that holds only `[]` both count as empty.
Reason: today an inbox file with no message holds `[]`, not 0 bytes. A reader must not wait for a file of 0 bytes.
The current observation in D1 gives the command that shows this state.

Known limits of this text:

- The text `[]` ties the guidance to the current JSON format of the inbox file. If the harness changes the format, the text can become wrong.
- The text does not cover a missing inbox file.

Known limit: an empty inbox file can mean only that the harness delivered the message. That it also means that the model read the message is not verified. The 2026-11-08 friction entry used the empty inbox as proof of the read. The shipped text says that the teammate has the message.

Evidence for these facts:

- A successful send does not show a read. NOT RE-DERIVED (conditions gone — the inbox file of the 2026-11-07 run does not exist).
- The late arrival of an idle notice: NOT RE-DERIVED (second-hand — the leader of the 2026-11-08 run observed it, not the author of this design). The source is `friction/2026-11-08-late-idle-notices-look-like-refusal.md`. The cause of the late arrival is not verified, so the shipped text gives no cause.
- On 2026-11-26, in team `session-781049cc`, idle notices of the teammates `friction-scribe` and `impl-precommit` arrived after their reports. NOT RE-DERIVED (second-hand — the team lead observed this, not the author of this design).
- In the same team, the `shutdown_approved` message of `impl-precommit` arrived after its pane and its config entry were gone. NOT RE-DERIVED (second-hand — the team lead observed this, not the author of this design).

### D6: Platforms and writes

The change touches no launch, process, or install code of the tool.

| Item | Windows | Linux and macOS |
|---|---|---|
| Shutdown check in the skill | PowerShell form, not tested | bash form, tested on Linux only |

The shipped commands only read files under `~/.claude/teams/`. This change adds no write outside the repository. The test and apply tasks write only inside the worktree.

### D7: Name `python3` in the README

`README.md` gets one sentence next to the tmux requirement: "The shipped `end-teammate` skill needs `python3` for its shutdown check on Linux and macOS."
The operator decided this. Reason: a user then learns about the new tool before a shutdown check fails.
The launcher does not check for `python3`, because this change adds no CLI behavior. This is documentation only, so it needs no requirement and no test.

## Risks / Trade-offs

- [On macOS without the Command Line Tools, `/usr/bin/python3` can open an install dialog.] This is a known limit. NOT RE-DERIVED (not run on macOS). The loop then does not confirm the shutdown.
- [A host has no `python3`.] The bash check ends with code 124 and shows the error. It never confirms the shutdown by mistake. The leader can still read the config file.
- [The harness changes the files under `~/.claude/teams/`.] They are not a stable interface. If the `members` list or the `name` field changes, the shutdown check does not confirm, and a person sees the timeout. If the harness changes how it uses the inbox files, the message state text can become wrong. No test finds this.
- [An empty inbox file does not prove that the model read the message.] This is the known limit in D5. The shipped text tells the reader to wait for the reply, and the reply is the proof.
- [A content test proves only that the text is present.] Task 1.5 runs the shipped bash command against fixtures.
- [The macOS system has no `timeout` command by default.] The earlier bash form also uses `timeout`, so this change adds no new risk. NOT RE-DERIVED (not run on macOS).

## Migration Plan

A user runs `claude-team --update <target>` to get the new text. The new hashes let `--dry-run` label an unedited installed file `older shipped version`. To roll back, revert the commit.
