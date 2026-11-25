# A Read in a worktree delivered rule files from two checkouts

- Old ID: F-006
- Date: 2026-11-08
- Observation: A session started in the main checkout. It then read two files in a worktree.
  The two Reads delivered the unscoped rules a second time, from the worktree copies.
  They also delivered rule copies from the worktree and from the main checkout.
  The main copies included a rule that the branch deleted (`dotnet/build-parallelism.md`).
- Evidence: `/home/flatrick/src/github/flatrick/claude-team/.scratch/agent-ledgers/rules-load-review/20261108T111516Z-rules-load-reviewer.md`, key `dual-checkout-rule-delivery-in-worktree`.
- Effect: A teammate sees the old main rule text next to the new worktree text.
  Rule edits of a branch do not govern sessions that started in main until the branch merges and the session restarts.
  The always-loaded rules cost double in a worktree.
- Not isolated: the reviewer did not find out which Read caused which delivery. A probe must read one file at a time.
- Cause: not verified.
- Status: open.

## Case 2026-11-25: a Write in a worktree delivered the startup rules again

- Claude Code version: 2.1.332 (Claude Code).
- Setup: team session-40a2bfec. Teammate `friction-scribe` (preset friction-scribe). Its startup rules came from the main checkout. Its worktree was `.worktrees/friction-log`.
- Observation: The scribe used the Write tool to make a new file in `friction/` of its worktree. The harness then delivered the worktree copies of the rules without a path filter.
  - Before the Write, the scribe saw no worktree copy. It did this work first: Bash calls that ran `cd` into the worktree, and Bash calls that printed files of the worktree.
  - The scribe made no Read tool call on a worktree file before the delivery.
  - The scribe saw no `CLAUDE.md` and no `AGENTS.md` from the worktree.
- Evidence: NOT RE-DERIVED (conditions gone — the delivery is only in the context of the scribe). The scribe saw it in its own context.
  On 2026-11-25, `diff -rq .claude/rules .worktrees/friction-log/.claude/rules` printed nothing.
- Cause: not verified.
