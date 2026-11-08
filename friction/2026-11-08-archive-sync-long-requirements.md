# The archive sync made two main-spec requirements too long, and only the post-archive validation showed it

- Old ID: F-012
- Date: 2026-11-08
- Claude Code version: 2.1.319.
- Setup: `versioning` run, change `versioning`. Commands: `/opsx:archive` (`openspec archive versioning -y`) and `openspec validate`.
- Observation: `/opsx:archive` synced the delta specs into the main specs.
  Then `openspec validate --all --strict` warned "Requirement text is very long (>500 characters)" for two main specs:
  - `tooling/install-tool/commands`, requirement `Status`.
  - `tooling/install-tool/install`, requirement `Install replaces the global tool`.
  `openspec validate versioning --strict` on the change had passed before the archive.
  The length check on the merged requirement text showed only after the sync.
- Evidence: the ledger entries in `/home/flatrick/src/github/flatrick/claude-team/.worktrees/versioning/.scratch/agent-ledgers/versioning/`:
  `20261108T200008Z-versioning-implementer-6.md` (key `impl-archive-specs-long-requirement-warnings`) and `20261108T195908Z-versioning-requirements-3.md` (keys `req3-status-requirement-too-long`, `req3-install-replaces-too-long`).
  Re-derive: `rg -n 'long-requirement|too-long' /home/flatrick/src/github/flatrick/claude-team/.worktrees/versioning/.scratch/agent-ledgers/versioning/`.
- Cause: the leader and the ledger entries state it. The scribe did not run the archive. NOT RE-DERIVED (second-hand — leader and `versioning-implementer` observed it).
- Effect: One extra teammate round and an amend of the archive commit.
- Idea (not decided): run `openspec validate --all --strict` on a trial archive before the real one.
- Status: open.
