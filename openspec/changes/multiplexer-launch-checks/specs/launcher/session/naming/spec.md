## MODIFIED Requirements

### Requirement: Sanitize default names

The tool SHALL replace each period, each colon, and each whitespace character with an underscore in a default session name, including the worktree part.
Source: `src/ClaudeTeam/SessionName.cs` (`Sanitize`), `tests/ClaudeTeam.Tests/SessionNameTests.cs` (`Sanitize_replaces_tmux_target_separators`, `Resolve_sanitizes_the_worktree_part_too`). The whitespace rule is in `Sanitize` (`char.IsWhiteSpace`); its test is `Resolve_replaces_whitespace_in_the_worktree_part` in `tests/ClaudeTeam.Tests/SessionNameTests.cs`.

#### Scenario: Period in the folder name

- **WHEN** the default name would be `my.repo`
- **THEN** the session name is `my_repo`

#### Scenario: Space in the folder name

- **WHEN** the default name would be `my repo`
- **THEN** the session name is `my_repo`

#### Scenario: Space in the worktree part

- **WHEN** the default name would be `repo+my tree`
- **THEN** the session name is `repo+my_tree`
