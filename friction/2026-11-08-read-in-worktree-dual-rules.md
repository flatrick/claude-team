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

## Case 2026-11-25: the cause of the double load

- Date: 2026-11-25
- Cause: verified (Claude Code 2.1.332, Read trigger).
- Summary:
  A session loads its start instructions from its start folder, and a Read in `.worktrees/<slug>/` adds the rules of that worktree.
  A `paths:` entry that ends in `/**` matches at any depth, so such main rules also load for worktree files.
- Details and limits: https://github.com/flatrick/claude-team/issues/16#issuecomment-7174314489
- Write trigger, not covered by this cause: a Write by `friction-scribe` in its worktree also delivered worktree rules. NOT RE-DERIVED (conditions gone — the delivery was only in the context of the scribe).
