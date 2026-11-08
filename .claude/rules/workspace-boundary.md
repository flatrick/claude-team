# Workspace boundary

## Definition

The **workspace** is the git repository root that was active when the user opened Claude.
All file write operations MUST stay within this boundary, unless the user explicitly approves
the write.

This rule applies to ALL tools: Write, Edit, Bash, and all MCP tools.

## Absolute boundary — never cross without explicit permission

You MUST ask the user before you write, modify, or delete any file outside the workspace
root. This includes the paths below. The list is not complete:

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
commands emit binary output. They produce non-source artefacts, so they require explicit
permission, also when the output path is inside the workspace. Confirm the destination
path with the user before you run them.

## Package manager and tool config files

Files that affect system-wide or user-wide behavior MUST NOT be created or modified without
explicit permission. These files persist across sessions and affect other projects. Examples:

- **npm**: `~/.npmrc`
- **pip**: `~/.pip/pip.conf`, `~/.config/pip/pip.conf`
- **NuGet**: `~/.nuget/NuGet/NuGet.Config`, `%APPDATA%\NuGet\NuGet.Config`
- **Cargo**: `~/.cargo/config.toml`
- **Git**: `~/.gitconfig`, `~/.config/git/config`
- Any `nuget.config`, `.npmrc`, `pip.conf`, etc. placed outside the repo root

## Git internals

Do not run `git config --global` or `git config --system`. Do not edit `.git/hooks/`.
Do not modify `.git/config`. Each of these actions needs explicit permission.

## Allowed exceptions

These repository commands can write outside the workspace without a prompt:

- `dotnet run --file scripts/install-tool.cs -- install`
- `dotnet run --file scripts/install-tool.cs -- reinstall`
- `dotnet run --file scripts/install-tool.cs -- uninstall`

They change the user-global dotnet tool store.
Announce the command to the team lead before you run it.

Test code can use the system temp directory through `Directory.CreateTempSubdirectory`.
See [test-temp-paths.md](test-temp-paths.md).

## Enforcement

No hook enforces this boundary.
This boundary is a prose convention.
Apply every rule above yourself.

## The "two attempts" rule

If an approach to a problem has not worked after **two total attempts**, stop and report.
Count all variations that target the same underlying blocker.

"Two attempts" means two distinct strategies, not two identical commands. A rephrase of the
same fix counts as one strategy. For example, five different env vars for the same root cause
are one strategy, not five. If you are not sure, count conservatively and stop sooner.
