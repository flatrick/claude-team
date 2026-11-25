# Two rules seem to disagree about dated stamps

- Date: 2026-11-25
- Claude Code version: 2.1.332 (Claude Code)
- Setup: team session-40a2bfec. Leader `team-lead`. Teammate `scratch-rule-writer` (preset requirements-standard, pane %5). Branch `docs/scratch-not-referenced`.
  The team session, the teammate, and the pane: NOT RE-DERIVED (second-hand — the team lead gave them, not me).
- Observation:
  1. `scratch-rule-writer` added a new rule section with the line `Added: 2026-11-25.`.
  2. The leader asked the writer to remove the line. The leader cited `no-inlined-change-history.md`, which forbids "Dated provenance stamps on ordinary claims".
  3. The writer then found the section "A new rule names what it replaces" in `authoring-current-truth-docs.md`.
     That section says that a new rule file, agent, gate, or required review step does one of two things. It names the rule that it replaces, or it has an `Added: <yyyy-mm-dd>` stamp on its section.
     "A rule that does neither does not land."
  4. The leader withdrew the request.
- Evidence:
  - The two rule texts: run `rg -n 'Dated provenance|Added: <yyyy' .claude/rules/no-inlined-change-history.md .claude/rules/authoring-current-truth-docs.md` in the main checkout. The scribe ran it at `4f9930c`.
  - No exception in the first rule: `rg -n 'authoring|Added|Updated' .claude/rules/no-inlined-change-history.md` returned no match at `4f9930c`.
  - The path filter: run `sed -n 1,6p .claude/rules/authoring-current-truth-docs.md`. At `4f9930c`, the rule had `.claude/rules/**`, `.claude/skills/**`, and `.claude/agents/**`.
  - A new `Added: 2026-11-25.` line on the branch: run `git log -p origin/main..docs/scratch-not-referenced | rg -n '^\+Added: '`. The scribe saw one match, with the branch at `71958a1`.
  - The order of the requests and what each teammate did: NOT RE-DERIVED (second-hand — the team lead, not me).
- Cause: inferred, not verified.
  - `no-inlined-change-history.md` does not say that the stamps of the second rule are an exception. A reader who has only the first rule in context can read it as a ban.
  - `authoring-current-truth-docs.md` has a path filter. The team lead infers that the leader did not have it in context. The scribe did not check the context of the leader.
  - The two texts possibly do not conflict. The ban names stamps "on ordinary claims". The second rule is about the stamp on the section of a new rule. No sentence that the scribe checked in the first rule makes this difference clear.
- Effect: One fixup round had no result. A reviewer can remove a required stamp. The second effect is a risk that the team lead gave. The scribe did not observe it.
- Status: open
