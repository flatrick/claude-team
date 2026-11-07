# Workspace boundary

## Definition

The **workspace** is the git repository root that was active when the user opened Claude.
All file write operations MUST stay within this boundary unless the user explicitly approves
otherwise.

This rule applies to ALL tools: Write, Edit, Bash, and all MCP tools.

## Absolute boundary — never cross without explicit permission

You MUST ask the user before writing, modifying, or deleting any file outside the workspace
root. This includes — but is not limited to:

**User profile directories**
- `~`, `$HOME`, `$USERPROFILE` and any subdirectory
- `$APPDATA`, `$LOCALAPPDATA` (Windows)
- `~/.config`, `~/.local`, `~/.cache` (Linux/macOS XDG)
- `~/Library` (macOS)

**Temporary directories**
- `/tmp`, `/var/tmp`, `/run` (Linux/macOS)
- `$TEMP`, `$TMP` (Windows)

**System directories**
- `/etc`, `/usr`, `/usr/local`, `/opt`, `/var` (Linux/macOS)
- `C:\Windows`, `C:\Program Files`, `C:\Program Files (x86)` (Windows)

**Any path that is not a descendant of the repo root.**

## Compiling executables

`dotnet publish`, `go build`, `cargo build`, `gcc -o ...`, `make install`, and similar
commands that emit binary output require explicit permission even when the output path is
inside the workspace, because they produce non-source artefacts. Confirm the destination
path with the user before running.

## Package manager and tool config files

Files that affect system-wide or user-wide behavior MUST NOT be created or modified without
explicit permission. These persist across sessions and affect other projects. Examples:

- **npm**: `~/.npmrc`
- **pip**: `~/.pip/pip.conf`, `~/.config/pip/pip.conf`
- **NuGet**: `~/.nuget/NuGet/NuGet.Config`, `%APPDATA%\NuGet\NuGet.Config`
- **Cargo**: `~/.cargo/config.toml`
- **Git**: `~/.gitconfig`, `~/.config/git/config`
- Any `nuget.config`, `.npmrc`, `pip.conf`, etc. placed outside the repo root

## Git internals

Do not run `git config --global` or `git config --system`, edit `.git/hooks/`, or modify
`.git/config` without explicit permission.

## Allowed exceptions

Test code may use the system temp directory through `Directory.CreateTempSubdirectory`.
See [test-temp-paths.md](test-temp-paths.md).

## Enforcement

No hook enforces this boundary.
This boundary is a prose convention.
Apply every rule above yourself.

## The "two attempts" rule

If an approach to solving a problem has not worked after **two total attempts** — counting
all variations targeting the same underlying blocker — stop and report.

"Two attempts" means two distinct strategies, not two identical commands. Rephrasing the
same fix (e.g. trying five different env vars to solve the same root cause) counts as one
strategy, not five. When in doubt, count conservatively and stop sooner.
