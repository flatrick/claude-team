# Friction log

This folder is the friction log. Each entry is one file.
An entry gives the observation, the evidence, and the effect on coordination.
The status of an entry is open until the operator makes a decision.

## Who writes it

- The `friction-scribe` teammate writes the entries for the team-leader session.
- Other sessions report friction to the leader. The leader forwards it to the scribe.

The entry format is in [`.claude/agents/friction-scribe.md`](../.claude/agents/friction-scribe.md).

## File names

Each entry is a file named `YYYY-MM-DD-<slug>.md`.
The date is the date of the entry. The slug is a short kebab-case form of the title.
The file name is the ID of the entry.

There is no index file. The folder listing is the index.

Reasons:

- With one log file, every two branches that add entries conflict at the end of the file.
- With sequential numbers, two machines with unmerged entries use the same next number.
- With separate files, a collision needs the same date and the same slug. Git then shows an add/add conflict on that one file.
- An index file is a new place for merge conflicts.

## Find entries

- List all entries: `ls friction/`.
- List the open entries: `rg -l -- '- Status: open' friction/`.
- Find an entry by its old number: `rg -l -- '- Old ID: F-003' friction/`.
