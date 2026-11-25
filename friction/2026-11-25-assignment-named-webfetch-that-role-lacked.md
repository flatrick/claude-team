# An assignment named WebFetch, and the teammate did not have the tool

- Date: 2026-11-25
- Claude Code version: 2.1.332 (Claude Code)
- Setup: team session-40a2bfec. Teammate `rules-probe-2` (preset troubleshooter-standard, pane %4).
  The team session, the teammate name, and the pane: NOT RE-DERIVED (second-hand — the team lead gave them, not me).
- Observation:
  - The assignment asked `rules-probe-2` to read the docs on `code.claude.com` with WebFetch.
  - `rules-probe-2` had no WebFetch tool. It used `curl`.
  - The report of `rules-probe-2` says that it saved the pages in its scratchpad.
  - The scribe also had no WebFetch tool. A ToolSearch call did not find one.
- Evidence:
  - The assignment text and the missing tool: NOT RE-DERIVED (second-hand — rules-probe-2 and the team lead, not me).
    On 2026-11-25, the scribe found no statement about a missing tool in the written results of `rules-probe-2`. NOT RE-DERIVED (conditions gone — those results are temporary files).
  - The use of `curl` and the saved pages: `rules-probe-2` stated that it fetched a docs page with `curl` and saved the pages in its scratchpad.
    NOT RE-DERIVED (second-hand — rules-probe-2, not me).
  - The preset: run `sed -n 1,15p .claude/agents/troubleshooter-standard.md` in the main checkout.
    On 2026-11-25, the frontmatter had no `tools` field and no `disallowedTools` field. Thus the frontmatter does not name WebFetch and does not remove it.
  - The check of the scribe: ToolSearch with the query `select:WebFetch` returned "No matching deferred tools found".
    To re-derive it, make the same call in a teammate session. The tool list of the scribe and its list of deferred tools did not name WebFetch.
    `.claude/agents/friction-scribe.md` also had no `tools` field. Re-derive it with `sed -n 1,8p .claude/agents/friction-scribe.md`.
- Cause: not verified.
  - The team lead inferred two possible causes. First, WebFetch is a deferred tool, and the assignment did not tell the teammate to load it. Second, the preset does not list the tool.
  - The frontmatter of the preset does not support the second cause.
  - The check of the scribe does not support the first cause for the scribe session. The scribe did not check the session of `rules-probe-2`.
  - The agent list of the scribe session has a `web-fetch` agent. Its text says that it is for a session that has no WebFetch tool.
    NOT RE-DERIVED (conditions gone — the list is in the context of the scribe session).
    The inference of the scribe, not verified: the harness does not give WebFetch to some sessions.
- Effect: The leader wrote an assignment that named a tool that the teammate did not have. The teammate used `curl` as a replacement. The team lead reported no harm.
- Status: open
