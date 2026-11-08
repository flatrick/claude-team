# launcher/session/naming Specification

## Purpose

This spec defines how `claude-team` names the multiplexer session and the Claude leader.
The default name comes from the git repository and worktree of the current folder. Each worktree gets its own leader, and peers can tell the leaders apart.

## Requirements

### Requirement: Explicit session name

The tool SHALL use the value of `--session` as the session name when it is given, and SHALL resolve a default name only when it is absent.
Source: `src/ClaudeTeam/Program.cs` (line 16).

#### Scenario: Explicit name

- **WHEN** the tool runs with `--session demo`
- **THEN** the session name is `demo`

### Requirement: Read git state

The tool SHALL read the git state of the current folder with `git rev-parse --path-format=absolute --show-toplevel --git-dir --git-common-dir`, and SHALL read the origin URL with `git remote get-url origin`.
The tool SHALL treat the folder as outside git when the first command fails or does not print exactly three lines.
The tool SHALL treat the origin as absent when the second command fails or prints nothing.
Source: `src/ClaudeTeam/Git.cs` (`Read`).

#### Scenario: Folder outside git

- **WHEN** `git rev-parse` exits with a nonzero code
- **THEN** the tool resolves names as for a folder outside git

### Requirement: Default name outside git

The tool SHALL use the name of the current folder, sanitized, as the session name when the current folder is not in a git repository.
Source: `src/ClaudeTeam/SessionName.cs` (`Resolve`), `tests/ClaudeTeam.Tests/SessionNameTests.cs` (`Resolve_uses_the_current_folder_outside_git`).

#### Scenario: Plain folder

- **WHEN** the tool runs in `/home/u/notes` outside git
- **THEN** the session name is `notes`

### Requirement: Default name in the main checkout

The tool SHALL use the repository name derived from the origin URL as the session name in a main checkout.
When the origin is absent or yields no name, the tool SHALL use the name of the folder that contains the common git directory.
Source: `src/ClaudeTeam/SessionName.cs` (`Resolve`), `tests/ClaudeTeam.Tests/SessionNameTests.cs` (`Resolve_uses_the_origin_name_in_the_main_checkout`, `Resolve_uses_the_main_checkout_folder_when_origin_is_absent`).

#### Scenario: Origin present

- **WHEN** the tool runs in a main checkout whose origin is `git@github.com:flatrick/claude-team.git`
- **THEN** the session name is `claude-team`

#### Scenario: Origin absent

- **WHEN** the tool runs in a main checkout at `/src/local` with no origin
- **THEN** the session name is `local`

### Requirement: Default name in a linked worktree

If the git directory differs from the common git directory, the tool SHALL name the session `<repository>+<worktree folder>`.
The tool SHALL resolve `<repository>` as in the main checkout. The tool SHALL use the last segment of the worktree top-level path as `<worktree folder>`.
Source: `src/ClaudeTeam/SessionName.cs` (`Resolve`), `tests/ClaudeTeam.Tests/SessionNameTests.cs` (`Resolve_appends_the_worktree_folder_in_a_linked_worktree`).

#### Scenario: Linked worktree

- **WHEN** the tool runs in the linked worktree `.worktrees/feature` of a repository whose origin names `claude-team`
- **THEN** the session name is `claude-team+feature`

### Requirement: Repository name from a remote URL

The tool SHALL derive the repository name from the last path segment of the remote URL.
First, the tool trims surrounding whitespace and trailing slashes or backslashes. Then it removes a trailing `.git` in any letter case.
For a URL with a scheme, only the part after the host SHALL count as the path.
The tool SHALL derive no name when nothing remains.
Source: `src/ClaudeTeam/SessionName.cs` (`FromRemoteUrl`), `tests/ClaudeTeam.Tests/SessionNameTests.cs`.

#### Scenario: Supported URL forms

- **WHEN** the origin is `https://github.com/o/repo/`, `ssh://git@host:2222/owner/repo.git`, `git@host:repo.git`, `/srv/git/repo.git`, or `C:\repos\repo`
- **THEN** the repository name is `repo`

#### Scenario: No name remains

- **WHEN** the origin is empty, whitespace only, `.git`, or `https://host/`
- **THEN** no repository name is derived

### Requirement: Sanitize default names

The tool SHALL replace each period and each colon with an underscore in a default session name, including the worktree part.
Source: `src/ClaudeTeam/SessionName.cs` (`Sanitize`), `tests/ClaudeTeam.Tests/SessionNameTests.cs` (`Sanitize_replaces_tmux_target_separators`, `Resolve_sanitizes_the_worktree_part_too`).

#### Scenario: Period in the folder name

- **WHEN** the default name would be `my.repo`
- **THEN** the session name is `my_repo`
