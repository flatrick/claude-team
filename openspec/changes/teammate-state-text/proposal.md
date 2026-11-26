## Why

The shipped team skills lead a leader to a wrong view of the state of a teammate. Three friction entries show this:

- `friction/2026-11-15-end-teammate-name-check-false-positive.md`: step 6 of `end-teammate` searches the whole team config for the name. A name in the `prompt` field of a different member matches. A correct shutdown then looks like a failed one.
- `friction/2026-11-07-sendmessage-treated-as-delivery.md`: the leader treated a successful `SendMessage` result as proof that the teammate read the message.
- `friction/2026-11-08-late-idle-notices-look-like-refusal.md`: an old idle notice arrived after a newer leader message and looked like a refusal.

## What Changes

- Step 6 of the shipped `end-teammate` skill checks only the names of the member entries in the team config. The bash form uses `python3 -I -c` with the standard `json` module. The PowerShell form uses `ConvertFrom-Json` and stays marked `not tested on Windows`.
- Both forms report that the name is gone only when they read a member list that does not have the name. If a form cannot read the config, it does not report this.
- Step 5 of the same skill says that no member entry can have the name.
- The shipped `team-protocol` skill gets one new section about the state of a message. It covers the meaning of a successful send, a message in an inbox file, and a late idle notice. For a late idle notice, the reader checks the inbox file of the teammate.
- The bash form of step 6 needs `python3`. `README.md` gets one sentence about this, next to the tmux requirement. The operator decided this. The reason is that a user then learns about the tool before a shutdown check fails.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `launcher/starter-files/content`: new requirements for the member name check in `end-teammate` and for the message state guidance in `team-protocol`.

## Impact

- Spec path touched: `openspec/specs/launcher/starter-files/content/spec.md`.
- Shipped sources: `src/ClaudeTeam/skills/end-teammate/SKILL.md`, `src/ClaudeTeam/skills/team-protocol/SKILL.md`, and `src/ClaudeTeam/shipped-hashes.txt`.
- Repository copies: the same two skills under `.claude/skills/`.
- Documentation: `README.md`.
- Tests: `tests/ClaudeTeam.Tests/ShippedContentTests.cs`.
- A user gets the new text with `claude-team --update`.

## Out of scope

The operator limited this change to a fix of the shipped text, with no new CLI command. The reason is that most causes are in the harness. Also, the files under `~/.claude/teams/` are not a stable interface. The one sentence in `README.md` documents the shipped text.

- A new command or option of `claude-team`.
- A change to the behavior of the harness: message delivery, idle notices, or the files under `~/.claude/teams/`.
- The other shipped files: the team-leader agent, `start-teammate`, and the peer-leader rule.
- Other changes to `README.md`.
- The friction entries.
- Other friction entries.
