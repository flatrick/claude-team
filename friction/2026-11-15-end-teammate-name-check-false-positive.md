# The name check in end-teammate step 6 gave a false positive

- Date: 2026-11-15
- Claude Code version: 2.1.325 (Claude Code)
- Setup: team session-5ddbdd7b. The leader shut down rider-implementer (preset implementer-standard, pane %1).
- Observation: The leader ran `timeout 40 bash -c 'until ! grep -q "rider-implementer" ~/.claude/teams/session-5ddbdd7b/config.json; do sleep 1; done'`.
  The command ended with exit code 124 (timeout).
  The shutdown had worked. Pane %1 was absent from `tmux list-panes -a`. `jq '.members[].name'` listed only team-lead and friction-scribe.
- Evidence:
  - Re-derive the match: `rg -n rider-implementer ~/.claude/teams/session-5ddbdd7b/config.json`. On 2026-11-15 it matched only line 32.
    Line 32 is the `prompt` field of friction-scribe, which names rider-implementer.
  - Re-derive the member list: `jq -r '.members[].name' ~/.claude/teams/session-5ddbdd7b/config.json`. On 2026-11-15 it printed team-lead and friction-scribe.
  - Re-derive the panes: `tmux list-panes -a`. On 2026-11-15 it listed %0 and %2 and not %1.
  - The 124 exit code: NOT RE-DERIVED (second-hand — the leader ran it, not me). The grep that the leader ran matches the same text as the `rg` command above.
  - The match goes away when the prompt leaves the config. After that, the `rg` command returns nothing. NOT RE-DERIVED (conditions gone) for that point.
- Cause: verified. The `grep` command searches the whole config file. It does not search only the member names. A teammate name in the `prompt` field of another member matches.
- Effect: A correct shutdown looked like a failed one. The wait added 40 seconds. The leader needed a manual `jq` check.
- Status: open
- Fix: Not applied. One possible check is `jq -e '.members[]|select(.name=="<name>")' <config>`. It tests the member names only.
