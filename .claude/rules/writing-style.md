# Writing style: ASD-STE100 Simplified Technical English

Added: 2026-11-08.

## Rule

Write text in this repository in ASD-STE100 (Simplified Technical English), so that readers with little English understand it easily.
This rule has no `paths:` filter. Because of this, every session loads it.
The rule covers every kind of text. It must reach every role before the role writes.
[AGENTS.md](../../AGENTS.md) ("Where instructions live") ranks reliable delivery above narrow audience. This file makes that trade-off.
The rule applies to all text in scope, existing text and new text. When you edit existing text, make it follow this rule.

### Text that follows this rule

- Rules, skills, and agent briefs.
- `AGENTS.md`, `CLAUDE.md`, and `README.md`.
- OpenSpec specs and change artifacts.
- Code comments and doc comments.
- Commit messages and PR text.
- Decision files.
- Reports and ledger entries.
- Your replies to the team lead and the operator.

### Writing rules

1. Write short sentences. In a procedure, use 20 words or fewer in a sentence. In a description, use 25 words or fewer.
2. Give one instruction in each sentence.
3. Use the active voice. Name the actor.
4. Use the imperative for an instruction: "Run the test", not "The test should be run".
5. Use one word for one meaning, and use the same word each time for the same thing. Do not vary a term for style.
6. Use a simple verb in place of a phrasal verb: "start", not "kick off"; "remove", not "take out".
7. Keep the articles "a", "an", and "the". Keep "that" in a clause where it helps the reader.
8. Put a sequence of steps in a numbered list. Put a set of items in a bulleted list.
9. Keep a paragraph to 6 sentences or fewer, and give one topic to a paragraph.
10. Put the condition before the action: "If the test fails, stop."
11. Use the simple present tense for facts and rules. Use the simple future only for a real future event.
12. Do not use idioms, jokes, or figures of speech.
13. Do not use a noun string of more than three nouns. Use a short phrase with a preposition instead.
14. Use a warning or a caution as a separate sentence before the step it covers.
15. Use words from the STE100 dictionary where you can. Use technical names as they are.
16. Outside the RFC-2119 keywords, use "must" and "can" with their normal meaning: "must" for a requirement and "can" for an ability.
    Do not use "should" or "may" in such text. For a recommendation, write "prefer X" or "we recommend X". For permission, write "you can".

### Exceptions

Do not change these. Write them exactly as the source gives them.

- Code, identifiers, commands, and paths.
- Output that a tool printed, when you quote it.
- Words of other people, when you quote them.
- The RFC-2119 keywords SHALL, MUST, SHOULD, and MAY in specs and rules.

### CLI output strings

User-visible CLI output strings and error messages follow the writing rules.
A change to such a string is a change to observable behavior, so it needs an OpenSpec change.
Do not change a CLI string in a documentation-only change.

## Why

A reader with little English, or a reader who works fast, misreads long sentences, passive voice, and varied terms.
A simple text has less room for a wrong reading.
An agent obeys a directive that it understands.

## Example

Good: "If the build fails, stop and report the error to the team lead."

Bad: "Should the build happen to fail, it may be advisable to halt and let the lead know what came up."

## Enforcement

Behavioral.
No tool checks the style of text.
Apply this rule when you write or edit text.
A reviewer checks each changed document against the rules above.
