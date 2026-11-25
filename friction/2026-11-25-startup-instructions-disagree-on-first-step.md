# The startup instructions disagree on which step is first

- Date: 2026-11-25
- Claude Code version: 2.1.332 (Claude Code)
- Setup: team session-40a2bfec. Leader `team-lead` (preset team-leader, in-process). Teammate `rules-probe` (preset general-purpose, pane %2). Teammate `friction-scribe` (preset friction-scribe).
- Observation: More than one startup instruction claims the first step.
  - Leader: `.claude/agents/team-leader.md` line 9 says "Invoke the skill `team-leader-local`". Line 10 says "Invoke the `team-protocol` skill before anything else."
    The `initialPrompt` on line 6 also says "Invoke the team-protocol skill before anything else."
    The `team-leader-local` skill says that the agent "invokes it as its first step".
    The leader also reported a harness reminder with the same text as line 6. The leader invoked `team-protocol` first.
  - Teammate: `.claude/skills/team-protocol/SKILL.md` line 6 says "Invoke the skill `team-protocol-local`". Line 20 says "Your first action is ToolSearch with the query `select:SendMessage`."
    The `team-protocol-local` skill says that `team-protocol` "invokes this skill first".
    `rules-probe` loaded `team-protocol`, then `team-protocol-local` and ToolSearch.
    `friction-scribe` made the `team-protocol-local` call and the ToolSearch call in one tool batch.
- Evidence:
  - Re-derive the text: `rg -n 'before anything else|first action|Invoke the skill' .claude/agents/team-leader.md .claude/skills/team-protocol/SKILL.md`. Run it in the main checkout at `4f9930c`.
  - Re-derive the claims of the two `-local` skills: `rg -n 'first step|skill first' .claude/skills/team-leader-local/SKILL.md .claude/skills/team-protocol-local/SKILL.md`.
  - The leader order and the harness reminder: NOT RE-DERIVED (second-hand — the leader saw them, not me).
  - The team session and the pane of `rules-probe`: NOT RE-DERIVED (second-hand — the leader gave them, not me).
  - The load order of `rules-probe`: NOT RE-DERIVED (second-hand — rules-probe saw it, not me). The team lead reported that the probe made the `team-protocol-local` call and the ToolSearch call in one call. The written statement of the probe that the scribe read did not say "one call".
  - The tool batch of `friction-scribe`: NOT RE-DERIVED (conditions gone — the startup of this session is complete). The scribe saw it in its own context.
- Cause: inferred, not verified. Each file calls its own step the first step. No file that the scribe checked gives one order for all the steps. The scribe checked only the files in this entry.
- Effect: Each role must guess the order. A strict reader can stop or skip a step. No step was skipped in the cases above.
- Status: open
