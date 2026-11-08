## MODIFIED Requirements

### Requirement: Install replaces the global tool

The `install` command SHALL install the build of the current checkout, whatever version is installed.
If another version or no version is installed, the `install` command SHALL run `dotnet tool update` with the option `--allow-downgrade`.
If the same version is installed, the command SHALL run `dotnet tool uninstall` and then `dotnet tool install`.
Each command runs with `--global claude-team`. The uninstall takes no source.
The update and the install add the pack folder as a source and the packed version.
A failed step stops the command with its exit code.
Source: `scripts/install-tool.cs` (`RunInstall`, `GetInstalledVersion`).

#### Scenario: Unchanged version

- **WHEN** version `0.1.0` is installed and the checkout packs `0.1.0`
- **THEN** the script uninstalls the tool and installs the new package

#### Scenario: Lower packed version

- **WHEN** version `0.1.1-alpha.0.7` is installed and the checkout packs `0.1.1-alpha.0.3`
- **THEN** the script runs `dotnet tool update` with `--allow-downgrade`
- **AND** version `0.1.1-alpha.0.3` is installed
- **AND** the script prints no warning that the new version is lower
