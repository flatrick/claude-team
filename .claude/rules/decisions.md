# Decision files

Use decision files to ask the user for decisions. Do not ask for decisions with long text in the chat.

## Location

- Write decisions in `.claude/decisions/<YYYY-MM-DD>.md` in the main checkout. Find it with `git worktree list --porcelain | head -1`. Use the local date.
- Make a new file when the date changes. This also applies after midnight in one session.
- `.claude/.gitignore` ignores `.claude/decisions/`. Git does not track these files.

## The files are temporary

- The user can delete a decision file at any time. A decision file is not a record.
- A decision is safe only when an artifact contains it. Record each answered decision in an artifact as soon as possible, after the user agrees.
- Do not use a decision file from an earlier session as a source of facts.

## No references to decision files

- Do not refer to a decision file from a different file. This applies to specs, designs, proposals, tasks, code, comments, commit messages, `CLAUDE.md` and memories.
- Do not write question IDs, links to `.claude/decisions/`, or text such as "as decided on <date>" in these files.
- When you record a decision in an artifact, write the decision and its reason in full. The artifact must be clear without the decision file.
- Only `.claude/.gitignore`, this rule file, and the exception in `artifacts-location-policy.md` give the name of the folder.

## Question IDs

- An ID is `Q`, then one hexadecimal digit, then a number.
- The digit is the day of the month modulo 16. For example, the 7th and the 23rd both give `7`, and the 26th gives `A`.
- The number starts at 0 on each day and increases by 1 for each question. For example: `Q70`, `Q71`, `Q710`.
- The first character after `Q` is always the day digit.

## File layout

```markdown
# Decisions <YYYY-MM-DD> (claude-team)

Open from earlier days: <links to open questions in earlier files, or "none">

## Open

### Q<d><n>: <title>
...

-------

## Answered

### Q<d><n>: <title>
...
```

- Put open questions under `## Open`, and answered questions under `## Answered`.
- Put one `-------` line between the two sections. Do not put lines between questions.
- When the user answers a question, move it to `## Answered`.
- A new file gives links to the questions that are still open in earlier files. Do not copy these questions.

## Question format

```markdown
### Q<d><n>: <title>

Status: open            <!-- open | decided | deferred -->

Context:
- One to three lines: what the question decides, and why it is necessary now.
- Links: <path to the related artifact or code>

Question: <one question>?

Options:

**A. <name> (recommended)**
- What: <what the option does>
- Why it may be relevant: <why the user can select this option>
- Cost: <what the option costs, or its risk>

**B. <name>**
- What: ...
- Why it may be relevant: ...
- Cost: ...

Answer:
<!-- The user writes the letter, and notes if necessary. -->

Captured in:
<!-- The agent writes the artifact that contains the decision. -->
```

- Each question has one recommended option or more.
- Each option has the three items "What", "Why it may be relevant" and "Cost".
- Label each option with one plain capital letter: A, B, C, D, and so on, in order. Do not use a variant such as "A'" or "D3". Do not use a number.
- When an option changes, for example after a review, replace the option in place and letter the list again. Do not keep the old option and the new option side by side.
- Reason: a variant label such as "A'" next to "A" does not look like a different option. The operator cannot tell the options apart.

## Procedure

1. Add the question to `## Open` in the file of the day.
2. In the chat, write one line only. For example: "Added Q77 (scan transaction scope)."
3. The user writes the answer under `Answer:` in the file, or in the chat (for example, "Q77: A").
4. Set `Status: decided`, write the answer in the file, and move the question to `## Answered`.
5. Record the decision in the artifact after the user agrees. Then fill in `Captured in:`.

## Enforcement

Behavioral.
No hook checks decision files.
Check the file layout and the references when you edit a decision file.
