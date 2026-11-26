# A teammate also appears in `ListAgents` as a Remote Control peer with a host-based name

- Old ID: F-008
- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run. Teammate `versioning-implementer` (preset `implementer-standard`) ran in tmux pane `%2` on host `death-angel`.
- Observation: `ListAgents` showed the teammate as a separate peer session with a host-based name, `death-angel-parallel-prism`.
  The leader took it for a possible peer leader.
  The leader paused a global install to ask it, as `claude-team-peer-leaders.md` requires.
  The peer answered with a cross-session message from `from-name="death-angel-parallel-prism"`.
  The harness routed the message as "a Claude session on another machine, over Remote Control".
  The peer named itself `versioning-implementer`.
- Evidence: the cross-session message in the leader session. The friction note of the run said: "A teammate also shows in `ListAgents` as a Remote Control peer session with a host-based name (`death-angel-parallel-prism` was `versioning-implementer`). The leader took it for a possible peer leader and paused a global install to ask it."
  It also said that the reply of the peer had `from-name="death-angel-parallel-prism"`, and that the peer named itself `versioning-implementer`.
  Spawn record, copied from the spawn log: teammate `versioning-implementer`, preset `implementer-standard`, stamp `20261108T191907Z`, pane `%2`, result "passed first time".
  Re-derive: run `ListAgents` while one pane teammate runs, and compare its rows with `~/.claude/teams/<team>/config.json`.
  Check of 2026-11-25 (Claude Code 2.1.332): the scribe ran `ListAgents` while it was the one pane teammate (pane `%3`).
  A teammate does not see its own row, so this run does not test the claim. The leader must run the step.
  In this run, the leader showed two times: as a teammate (`team-lead`) and as a peer session (`claude-team`, tmux pane `%0`).
  Leader run of 2026-11-25: to re-derive it, run `ListAgents` from the leader session while a pane teammate runs. This is one run on one version (2.1.332 in the scribe session).
  The output that the team lead quoted: "This session is claude-team [5cc809] — the name other sessions use to message it (it is not listed below; a message to it would be a message to yourself). Teammates (1): friction-scribe [2bda67] · friction-scribe · pane · started 28m ago".
  The team lead reported that the pane teammate showed only under "Teammates", not as a peer session. The quote does not include a list of peer sessions.
  NOT RE-DERIVED (second-hand — the team lead ran it, not the scribe). One run does not show that the earlier observation cannot occur again.
  NOT RE-DERIVED (second-hand — the leader observed the `ListAgents` row and the message; the friction scribe did not).
- Second case: before 20:07:50 UTC (the time of the reply of `friction-scribe`), `ListAgents` showed one online peer, `death-angel-curried-peacock` (Remote Control, idle).
  The leader asked it for an acknowledgment before a reinstall from main.
  It answered by cross-session message (`from-name="death-angel-curried-peacock"`) that it is `friction-scribe`, a teammate.
  The bridge name is new for each teammate (`versioning-implementer` was `death-angel-parallel-prism`).
  The check costs one round trip each time.
- Third case: `ListAgents` showed a new online peer, `death-angel-breezy-squid`.
  The leader sent a hold and asked it. The peer was `release-v020` itself.
  The cross-session reply came from `from-name="death-angel-breezy-squid"`.
  NOT RE-DERIVED (second-hand — the leader saw the row and the reply; the scribe did not).
  NOT RE-DERIVED (second-hand — the leader saw the row; the scribe sent the reply, see the reply text in this session).
  Check of 2026-11-25: "this session" is an earlier session. The reply text: NOT RE-DERIVED (conditions gone — that session ended).
- Fourth case (2026-11-26, Claude Code 2.1.332 in the scribe session):
  - Setup: team `session-781049cc`. The leader `claude-team` (preset `team-leader`) ran in the main checkout. Teammate `friction-scribe` (preset `friction-scribe`) ran in tmux pane `%1` on host `death-angel`.
  - Observation of the leader: before it removed a worktree, the leader ran `ListAgents`.
    Under "Teammates", the output listed `friction-scribe [9d7051] · pane`.
    It also listed a new peer, `death-angel-lively-milner [0879f3] · Remote Control · idle`.
    The first `ListAgents` of the session, before the start of the scribe, did not show this peer.
    The `SendMessage` result for the peer said "a Claude session on another machine, over Remote Control; not confirmed read ... this route reports nothing back".
    The peer answered with a cross-session message from `bridge:session_01VBVYU3scZXytq1ctgogP8K`, and said that it is `friction-scribe`.
    NOT RE-DERIVED (second-hand — the team lead saw the `ListAgents` output, the send result, and the reply; the scribe did not).
  - Observation of the scribe: a peer-check message arrived as a cross-session message with `from="bridge:session_01VBVYU4S2DV3PfhkZcexZWf"`, `from-name="claude-team"`, and `from-mode="prompting"`.
    The message said that the leader planned to remove the worktree `.worktrees/review-fixes`. It asked if the scribe is a team leader or a teammate.
    The scribe replied to the `from` address that it is the teammate `friction-scribe`.
    The send result for the reply said "delivery is not confirmed" and "this route reports nothing back".
    NOT RE-DERIVED (conditions gone — the message and the send result are only in the conversation of the scribe session).
  - The session ID in the reply that the leader got is the same ID as in the `Claude-Session` line that the harness gave the scribe for its commits.
    The line: `git log -1 --format=%B ed11e05` on the branch `docs/friction-log`.
    This indicates that the peer was the scribe (inferred, not verified).
  - Cause: not verified.
  - Effect: the peer rule made the leader send a peer check to its own teammate and wait before the removal.
    The cost was one round trip. There was no harm.
    The route text "another machine" was wrong again for a pane on the same host.
- Cause: not verified.
- Effect: The leader lost time and paused a global install. A teammate cannot be told from a peer leader by its name or by its route.
  The route text ("another machine") is wrong for a pane on the same host.
- Status: open.
