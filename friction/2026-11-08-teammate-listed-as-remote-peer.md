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
- Evidence: the cross-session message in the leader session. Source: `/home/flatrick/src/github/flatrick/claude-team/.scratch/versioning/friction-pending.md`, item 1.
  Spawn record: `rg -n 'versioning-implementer' /home/flatrick/src/github/flatrick/claude-team/.scratch/agent-spawn-log.md` (pane `%2`).
  Re-derive: run `ListAgents` while one pane teammate runs, and compare its rows with `~/.claude/teams/<team>/config.json`.
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
- Cause: not verified.
- Effect: The leader lost time and paused a global install. A teammate cannot be told from a peer leader by its name or by its route.
  The route text ("another machine") is wrong for a pane on the same host.
- Status: open.
