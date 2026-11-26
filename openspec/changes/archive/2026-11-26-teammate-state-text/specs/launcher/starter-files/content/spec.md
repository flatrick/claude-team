## ADDED Requirements

### Requirement: Shutdown check reads member names only

The wait step of the shipped `end-teammate` skill SHALL read the `members` list of the team config. It SHALL compare the teammate name only with the `name` field of each entry in that list.
It SHALL NOT search the whole config text, and SHALL NOT use `jq`.
The bash form SHALL run `python3 -I -c` with the standard `json` module inside a `timeout 40` wait loop. The PowerShell form SHALL use `ConvertFrom-Json` and carry the words `not tested on Windows`.

#### Scenario: Name in the prompt of a different member

- **WHEN** the teammate has no member entry, and the `prompt` field of a different member contains its name
- **THEN** the shipped bash check reports that the name is gone

#### Scenario: Name of a member entry

- **WHEN** a member entry has the teammate name until the time limit ends
- **THEN** the shipped bash check ends with the exit code of the `timeout` command, which is 124

#### Scenario: Shipped wait step

- **WHEN** the embedded `end-teammate` skill is read
- **THEN** its wait step has a `python3 -I -c` form and a `ConvertFrom-Json` form with the words `not tested on Windows`
- **AND** the skill contains no `grep` command and no `jq` command

Source: no code yet. This change adds the text to `src/ClaudeTeam/skills/end-teammate/SKILL.md` and the tests to `tests/ClaudeTeam.Tests/ShippedContentTests.cs`.

### Requirement: Shutdown check does not confirm on a read error

Each form of the wait step SHALL report that the name is gone only after it reads a `members` list without the name. This rule applies to the bash form and the PowerShell form of the shipped `end-teammate` skill.
If a form cannot read or parse the config, it SHALL continue to wait until the time limit.
The skill SHALL tell the leader which result confirms that the name is gone, and that any other result does not confirm it.

#### Scenario: Config cannot be read

- **WHEN** the config file is missing, empty, or not complete JSON until the time limit ends
- **THEN** the shipped bash check does not end with exit code 0

#### Scenario: Result of the wait step

- **WHEN** the embedded `end-teammate` skill is read
- **THEN** it says that exit code 0 of the bash form, or the output `True` of the PowerShell form, confirms that the name is gone

Source: no code yet. This change adds the text to `src/ClaudeTeam/skills/end-teammate/SKILL.md` and the tests to `tests/ClaudeTeam.Tests/ShippedContentTests.cs`.

### Requirement: A successful send is not a read

The shipped `team-protocol` skill SHALL say that a successful `SendMessage` result means that the message reached the inbox of the recipient. It SHALL say that this result does not mean that the recipient read the message.
It SHALL give the path of the inbox file of a member. It SHALL say that a message that is still in that file is not yet read.

#### Scenario: Message in the inbox file

- **WHEN** the embedded `team-protocol` skill is read
- **THEN** it says that a successful send does not show that the recipient read the message
- **AND** it says that a message that is still in the inbox file is not yet read

Source: no code yet. This change adds the text to `src/ClaudeTeam/skills/team-protocol/SKILL.md` and the tests to `tests/ClaudeTeam.Tests/ShippedContentTests.cs`.

### Requirement: Late idle notice checked against the inbox

The shipped `team-protocol` skill SHALL tell the reader what to do with a late idle notice. A late idle notice arrives after the reader sent a newer message to that teammate.
The reader SHALL read the inbox file of the teammate.
If the file is empty or holds only `[]`, the skill SHALL say that the teammate has the message. It SHALL tell the reader to wait for the reply, and not to treat the notice as a refusal.

#### Scenario: Late idle notice

- **WHEN** the embedded `team-protocol` skill is read
- **THEN** it tells the reader to read the inbox file of the teammate when an idle notice arrives after a newer message
- **AND** it says that, with a file that is empty or holds only `[]`, the teammate has the message
- **AND** it says that the reader then waits for the reply and does not treat the notice as a refusal

Source: no code yet. This change adds the text to `src/ClaudeTeam/skills/team-protocol/SKILL.md` and the tests to `tests/ClaudeTeam.Tests/ShippedContentTests.cs`.
