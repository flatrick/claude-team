## 1. Shutdown check in `end-teammate`

- [ ] 1.1 Load the `dotnet-workflow` skill. Red test: in `tests/ClaudeTeam.Tests/ShippedContentTests.cs`, add a test for the requirement "Shutdown check reads member names only". Assert that the `end-teammate` text contains the exact bash command of design D4. Assert that it contains `ConvertFrom-Json`, `.members.name`, and `not tested on Windows` on the PowerShell step. Assert that it contains no `grep`, no `jq`, and no `-match '<name>'`. Also assert the new second condition of step 5. Run the test and confirm that it fails.
- [ ] 1.2 Red test: add a test for the requirement "Shutdown check does not confirm on a read error". Assert the sentences of D4 that say which exit code and which output confirm that the name is gone. Assert that the PowerShell command contains `catch { $gone = $false }` and `[bool]$names`. Run the test and confirm that it fails.
- [ ] 1.3 Change steps 5 and 6 of `src/ClaudeTeam/skills/end-teammate/SKILL.md` as design D4 says. Confirm that the tests of 1.1 and 1.2 pass.
- [ ] 1.4 Copy the shipped source to `.claude/skills/end-teammate/SKILL.md`. Add the SHA-256 of the new LF text to `src/ClaudeTeam/shipped-hashes.txt` as a new line `skills/end-teammate/SKILL.md <hash>`. Keep each earlier line. Get the hash with `sha256sum src/ClaudeTeam/skills/end-teammate/SKILL.md`, or from the failure message of `ShippedHashesTests`. Confirm that `ShippedCopyTests` and `ShippedHashesTests` pass.
- [ ] 1.5 Run the bash command from the changed skill file against fixtures under `.scratch/` in the worktree. Replace the config path and `<name>`, and use `timeout 3`. Fixtures: the name only in the `prompt` of a different member (expect 0); the name in a member entry (expect 124); the JSON text `{"members":[` (expect 124); an empty file (expect 124); a path with no file (expect 124). Record each exit code in the report. If `pwsh` is available, run the PowerShell command in the same way and expect `True` only for the first fixture. Keep the label `not tested on Windows`.

## 2. Message state guidance in `team-protocol`

- [ ] 2.1 Red test: in `ShippedContentTests.cs`, add a test for the requirement "A successful send is not a read". Assert in the `team-protocol` text the section heading `## Message state`, the two sentences of design D5 about a send, the inbox file path, and the sentence "A message that is still in this file is not yet read." Run the test and confirm that it fails.
- [ ] 2.2 Red test: add a test for the requirement "Late idle notice checked against the inbox". Assert the three sentences of design D5 about the idle notice. Assert that the section `## Message state` contains no `timestamp`. Run the test and confirm that it fails.
- [ ] 2.3 Add the section of design D5 to `src/ClaudeTeam/skills/team-protocol/SKILL.md` after the section "The assignment". Add this text to no other shipped file. Confirm that the tests of 2.1 and 2.2 pass, and that `Shipped_text_has_no_repository_conventions` passes.
- [ ] 2.4 Copy the shipped source to `.claude/skills/team-protocol/SKILL.md`. Add the new hash line `skills/team-protocol/SKILL.md <hash>` to `src/ClaudeTeam/shipped-hashes.txt`, and keep each earlier line. Confirm that `ShippedCopyTests` and `ShippedHashesTests` pass.

## 3. README

- [ ] 3.1 Add the sentence of design D7 to `README.md`, directly after the paragraph that starts "The launcher needs tmux 3.2 or newer". Verify with `rg -n -A2 'needs tmux 3.2 or newer' README.md`.

## 4. Verify

- [ ] 4.1 Run the gate of the `dotnet-workflow` skill: a clean build, then the full test run of `ClaudeTeam.slnx`. Read both logs. Confirm that each test passes.
- [ ] 4.2 Run `openspec validate teammate-state-text --strict`. Confirm that it passes with no warnings.
- [ ] 4.3 Run `openspec validate --all --strict` and `openspec validate --archived`. Confirm that both pass with no warnings. Put the output in the report.
- [ ] 4.4 Report what a person must still check: the PowerShell command on Windows, and the bash command on macOS, also without the Command Line Tools.

## Workflow follow-up

- Archive the change in the worktree before the branch merges. At archive time, replace each "Source: no code yet" line in the synced spec with the source files and the test names.
