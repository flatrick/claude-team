# Nothing makes a subagent of a worker load the protocol skills

- Old ID: F-007
- Date: 2026-11-08
- Observation: The agent briefs allow short-lived subagents.
  `team-protocol` applies to "every teammate and subagent that sends messages".
  No text tells a worker to give `team-protocol`, the ledger path, or `dotnet-workflow` to a built-in subagent type.
  It is not tested whether a subagent receives the project rules.
- Evidence: `/home/flatrick/src/github/flatrick/claude-team/.scratch/agent-ledgers/rules-load-review/20261108T111516Z-rules-load-reviewer.md`, key `subagent-delivery-unspecified`.
  Method: `rg --hidden --glob '!.git/**' -n 'subagent' .claude` and a read of `team-protocol/SKILL.md`, section "Subagents".
- Effect: A subagent can run without the protocol, the ledger duty, or the build workflow.
  A probe with a general-purpose subagent must show which rules it receives.
- Cause: not verified.
- Status: open.
