# Decision options with variant labels were hard to read

- Date: 2026-11-09
- Claude Code version: 2.1.319 (Claude Code).
- Setup: Team-leader session `claude-team` in the main checkout, during `/opsx:explore` for the ship-team-skills topic. No teammate took part. Rule: `.claude/rules/decisions.md`.
- Observation: After a teammate review, the leader revised question Q91 in the decision file of the day.
  The leader kept the old option "A".
  It added "A'" (the reviewed variant) and "D3" (the label that the reviewer used for a new option) next to "B" and "C".
  The operator wrote: "the option-list is really hard to read; "A" and "A'" does NOT look like two DIFFERENT options. The choices should just be simple A-B-C-D and so on".
  The leader then re-lettered Q91 as A-D and saved a leader memory: use plain letters, and revise options in place.
- Evidence: The words of the operator above are exact.
  The decision file is untracked and temporary, so this entry does not link it (see `.claude/rules/decisions.md`, "No references to decision files").
  NOT RE-DERIVED (conditions gone — the leader rewrote the option list in place, so the layout with "A'" and "D3" no longer exists).
  The rule text on option letters: `rg -n -i 'letter|recommended' .claude/rules/decisions.md`.
  It shows the template `**A. <name> (recommended)**`. It has no instruction to use plain letters or to revise options in place.
- Cause: inferred, not verified. The leader copied the labels of the reviewer into the list for the operator without re-lettering.
- Effect: The operator could not tell the options apart. The operator had to ask for a rewrite before the operator could answer.
- Open questions: Does `.claude/rules/decisions.md` need a sentence that says "label options A, B, C, D in sequence, and revise options in place"?
  The scribe does not edit the rule. The operator decides.
- Status: open
