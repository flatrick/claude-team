# Git worktree workflow

## Rule

- The **repo-root checkout is the `main` working copy**. It MUST stay on the `main` branch.
  Never `git switch` / `git checkout` another branch in it. Never rename or relocate the
  repo-root directory. Do branch work in a worktree, never in the main checkout.

- All other branch work MUST use a linked worktree under **`.worktrees/<slug>/`**.

- `<slug>` is the topic name. The branch is `<type>/<slug>`. The worktree folder is
  just the `<slug>`, without the type prefix. Example: branch
  `feat/peer-rule-test` → folder `.worktrees/peer-rule-test/`.

- **Exception.** A worktree MAY be created somewhere other than `.worktrees/` only when
  a specific need requires it (for example, a tool that cannot operate on a nested path). State
  the reason when you do this. The default is always `.worktrees/<slug>/`.

## Creating a worktree

```bash
git worktree add -b <type>/<slug> .worktrees/<slug>
```

When you dispatch an agent into a worktree that you prepared with this command, give the agent
the path of that worktree. Do not also request automatic worktree isolation.
Manual preparation and host-managed isolation are separate workflows. Use only one for a dispatch.

### Confirm which build is installed

Added: 2026-11-07.

One global install serves every session on the machine.
The code of a worktree is live only after `dotnet run --file scripts/install-tool.cs -- install` from that worktree.
Installing from one worktree changes the `claude-team` command for every other session.
Ask the other leaders first, as [claude-team-peer-leaders.md](claude-team-peer-leaders.md) requires.

Check the installed build without a change:

```bash
dotnet run --file scripts/install-tool.cs -- status
```

The output names the installed version and the branch of this checkout.
A running leader keeps its old build until you restart it.

### `core.hooksPath` must be relative, not an absolute path into one checkout

`core.hooksPath` is in the shared `.git/config` that every worktree of this repository reads.
If the repository uses committed hooks, set it to the **relative** value `.githooks`, never to an
absolute filesystem path. A relative path resolves against its own copy of
`.githooks/` in each worktree. A hook fix that you commit on a branch then governs the commits of that branch.
An absolute path makes every worktree run the hooks of one fixed checkout.

Confirm the value directly:

```bash
git config --get core.hooksPath
```

Do not set this with `--global` or `--system`.
[workspace-boundary.md](workspace-boundary.md) requires explicit permission before you touch `.git/config`.

## Do not `cd` to the repo root for read-only surveys — the shell CWD repoints the next build

The Bash shell's working directory **persists between calls**. A read-only survey command that does
`cd <repo-root>` leaves the CWD at the main checkout. The *next* `dotnet build` / `dotnet test`
with no explicit `cd` then runs against **main**, not the worktree. Main is clean, so the command reports
"Build succeeded" while it tests none of your edits. Scope read-only surveys with
`git -C <worktree>` or absolute paths instead of `cd`. Or `cd` back to the worktree before any
build or test. Confirm that a build saw your changes with `git -C <worktree> diff --stat`.

The same risk applies to any throwaway `cd` away from the worktree.
Use a subshell, for example `(cd /tmp/x && dotnet build)`, so that the CWD of the persistent shell never leaves the worktree.

## Verify an edit landed in the right checkout, not just that the tool call succeeded

A tool call that reports success shows that the edit was accepted, not which checkout received it.
After any batch of edits to files that matter, run a command scoped to the current checkout:

```bash
git -C <path-to-active-worktree> diff --stat -- <files-you-just-edited>
```

An empty diff on a file that you just edited is a signal to stop and find the write. If another checkout
contains the write, do not discard that copy. Compare any different content. Keep the intended result
in the active worktree. Restore the other checkout to clean only after the recovery is complete.

## Cleanup

Before you remove a worktree, follow [claude-team-peer-leaders.md](claude-team-peer-leaders.md).
After the branch has merged, remove the worktree and prune its metadata:

```bash
git worktree remove .worktrees/<slug>
git worktree prune
```

## Notes

- The root `.gitignore` ignores `.worktrees/`, so the contents of nested worktrees never
  show in the `git status` of the main checkout.
- Worktrees under `.worktrees/` (inside the repo root) satisfy
  [workspace-boundary.md](./workspace-boundary.md): every write stays a descendant of the repo
  root.
- Each worktree has its own `bin/obj`, so a build in a worktree never clashes with a build in the
  main checkout.
- Commit from the worktree itself, not from the main checkout. Do not assume that sibling worktrees are
  disposable. Do not manually delete the `.git` file or the lock files of a worktree. Use
  `git worktree remove`.
