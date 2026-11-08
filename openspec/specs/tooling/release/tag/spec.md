# tooling/release/tag Specification

## Purpose
This spec defines the `tag` mode of `scripts/release.cs`. The mode creates the release tag on the commit that is already on `origin/main`, because a rebase merge writes new SHAs and a tag on a branch commit is lost.

## Requirements

### Requirement: Tag only a merged release commit

The `tag` mode SHALL create an annotated tag `vX.Y.Z` on `HEAD` only when all of these are true:

- the checkout is on the `main` branch and the working tree is clean
- `HEAD` equals `origin/main` after the script fetches `origin`
- the subject of `HEAD` is `chore(release): vX.Y.Z`
- the tag `vX.Y.Z` does not exist

Otherwise the mode SHALL create no tag, print the failed check to standard error, and exit with code 1.
Source: `scripts/release.cs` (`RunTag`).

#### Scenario: Merged release commit

- **WHEN** `HEAD` equals `origin/main` and its subject is `chore(release): v0.2.0`
- **THEN** the script creates the annotated tag `v0.2.0` on `HEAD`
- **AND** the exit code is 0

#### Scenario: Local commits not on origin

- **WHEN** `HEAD` is ahead of `origin/main`
- **THEN** no tag is created
- **AND** the exit code is 1

#### Scenario: Not a release commit

- **WHEN** the subject of `HEAD` is `feat(cli): add a flag`
- **THEN** no tag is created
- **AND** the exit code is 1

#### Scenario: Tag exists

- **WHEN** the tag `v0.2.0` already exists and `HEAD` has the subject `chore(release): v0.2.0`
- **THEN** the script changes no tag and exits with code 1

### Requirement: Tag never pushes

The `tag` mode SHALL NOT push. After it creates the tag, it SHALL print the command that the operator runs to push the tag: `git push origin vX.Y.Z`.
Source: `scripts/release.cs` (`RunTag`, the final `Console.WriteLine`).

#### Scenario: After a tag

- **WHEN** the script creates the tag `v0.2.0`
- **THEN** standard output shows `git push origin v0.2.0`
- **AND** `origin` has no new tag
