---
paths:
  - ".claude/rules/**"
  - ".claude/skills/**"
  - ".claude/agents/**"
---

# Authoring rules, skills, and agent briefs

## Rule

Every rule file, skill, and agent brief in this repository SHALL state its directive in the first sentence.
Use the imperative.
Include only the supporting prose that makes the directive unambiguous.
Each loaded document adds to the session's context.

Write **simple, unambiguous, and to the point.** If a sentence does not change what the reader does
next, cut it.

## Per-document shape

### Rule file (`.claude/rules/*.md`)

1. Title: one line. It names the thing that the rule governs, not the incident that caused it.
2. `## Rule`: the SHALL/MUST statement. The first sentence carries the verb. Do not write an introduction.
3. `## Why`: 1-4 sentences. State the failure mechanism, not a story. If a specific past incident
   is the reason, cite it as **one line with a link**, never a narrative. See
   [no-inlined-change-history.md](no-inlined-change-history.md). Link only to a file that is already committed.
4. `## Example` (optional): exactly one good example. Add one bad example only if the mistake is not obvious.
   Keep it minimal. Do not list every way that this has gone wrong.
5. `## Recurrence log` (optional): a bounded table with one row for each real occurrence:
   `date · where · one clause · link`. Never write a paragraph. See the worked example below.
6. `## Enforcement`: one paragraph. State if the rule is mechanical, review-time, or behavioral, and how a reader
   can tell that someone skipped it.

**Size is a warning sign, not a target.** If a rule is more than ~150 lines or ~1200 words, split
it, cut it, or move detail to the file that the recurrence log links to. Do not keep writing.

### Skill (`SKILL.md`)

Write the procedure first: numbered steps that an agent executes, not an essay about the domain. Write one line of
motivation at most. Examples show input and output, not narrative.

## Scoping a rule with `paths:` frontmatter

A rule that applies only to specific paths SHALL carry `paths:` YAML frontmatter that names those paths.
Scope test and build rules to their test files, project files, and scripts.
Scope code rules to `src/**` and `*.cs`.
Scope document-authoring rules to the rule, skill, and agent files that they govern.

**Do not scope a rule whose entire job is to catch work that leaves its expected paths.**
Rules like `workspace-boundary.md` must load wherever the work happens. If they do not, they can
never fire for the one case that they exist to catch.

## Before merging or renaming a rule file

First check who cites the file by its exact filename: `rg -l --hidden -g '!.git' "<filename>"` across
`.claude/agents/` and `tests/`. An agent brief, a hook, or a test comment can cite a rule by
name. Do not merge or rename such a rule before you update every citation. The token savings from merging small files are usually smaller than
the cost of those updates. Prefer to leave files as they are when they have similar topics but separate citations.

## A new rule names what it replaces

A new rule file, agent, gate, or required review step SHALL name the rule it replaces (the same change deletes or narrows that rule), or SHALL carry an "Added: <yyyy-mm-dd>" stamp on its section.
A later material edit adds or replaces an "Updated: <yyyy-mm-dd>" stamp.
A rule that does neither does not land.
Review each section whose newest Added or Updated date is more than 3 months old. Keep it, narrow it, or delete it. Add "Updated: <yyyy-mm-dd>" to a section that you keep.

Why: every incident or review adds process, and nothing removes it, so process only grows.

## Anti-patterns

- **A new paragraph that someone appends every time a mistake recurs.** This is the pattern that prompted this
  rule. See the worked example below.
- **Restating the same fact in prose across multiple files.** The section "measured fact belongs in one place" in
  [repo-conventions.md](repo-conventions.md) already bans this for counts. The same
  applies to a restated incident, a restated rationale, and a restated example.
- **Narrating the edit history of the document inline.** [no-inlined-change-history.md](no-inlined-change-history.md)
  already governs this. Do not duplicate its content here. Link to it.
- **Hedge words on a SHALL/MUST**: "generally," "in most cases," "usually," "consider," "try to."
  A directive is unambiguous, or it is not a directive.
- **An "evidence" section whose only job is to convince the reader that the rule is real.** A citation
  does that job in one line. A paragraph does not do it better. It costs more to read.

## Worked example

**Bad** (a paragraph appended after each recurrence):

> **Neither clause stopped it a third and fourth time.** On the `launch-retry` task, a
> coordinator wrote a disposition that cited this rule's "copy verbatim" text, and keyed three of
> five findings with fresh paraphrases anyway. The next disposition repeated it.

**Good** (the directive stays, and the narrative becomes one recurrence-log row):

> **Copy each finding key verbatim from the reporter's index table.**
>
> Why: a paraphrased key leaves the original finding open forever.
>
> | date | subject | link |
> |---|---|---|
> | 2026-11-01 | launch-retry | `.scratch/agent-ledgers/launch-retry/` |

The directive is identical in both. The good version is shorter and records an occurrence in one row.

## Enforcement

Behavioral, as [no-inlined-change-history.md](no-inlined-change-history.md) is. No regex
can separate a load-bearing "Why" sentence from padding. Apply the rule when you
write or edit a document. The keep-or-cut test is: *would a reader do anything differently because of this
sentence?* If not, cut it. The review that already runs over the document, or a human pass, checks the rule. No dedicated gate checks it.

## Scope

This rule applies to `.claude/rules/**`, `.claude/skills/**`, and `.claude/agents/**` in this repository.
Ledgers and retrospectives hold historical narratives.
See [no-inlined-change-history.md](no-inlined-change-history.md) for the history rule.
