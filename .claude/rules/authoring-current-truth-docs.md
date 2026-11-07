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

Write **simple, unambiguous, and to the point.** If a sentence doesn't change what the reader does
next, cut it.

## Per-document shape

### Rule file (`.claude/rules/*.md`)

1. Title — one line, names the thing being governed, not the incident that caused it.
2. `## Rule` — the SHALL/MUST statement. First sentence carries the verb. No throat-clearing.
3. `## Why` — 1-4 sentences. State the failure mechanism, not a story. If a specific past incident
   is the reason, cite it as **one line with a link**, never a narrative — see
   [no-inlined-change-history.md](no-inlined-change-history.md). Link only to a file already committed.
4. `## Example` (optional) — exactly one good and, only if the mistake is non-obvious, one bad.
   Minimal. Not a gallery of every way this has gone wrong.
5. `## Recurrence log` (optional) — a bounded table, one row per real occurrence:
   `date · where · one clause · link`. Never a paragraph. See the worked example below.
6. `## Enforcement` — one paragraph: mechanical, review-time, or behavioral, and how a reader
   would tell it was skipped.

**Size is a smell, not a target.** A rule pushing past ~150 lines / ~1200 words is a sign to split
it, cut it, or move detail into whatever the recurrence log links to — not to keep writing.

### Skill (`SKILL.md`)

Procedure first: numbered steps an agent executes, not an essay explaining the domain. One line of
motivation at most. Examples show input → output, not narrative.

## Scoping a rule with `paths:` frontmatter

A rule that applies only to specific paths SHALL carry `paths:` YAML frontmatter that names those paths.
Scope test and build rules to their test files, project files, and scripts.
Scope code rules to `src/**` and `*.cs`.
Scope document-authoring rules to the rule, skill, and agent files that they govern.

**Do not scope a rule whose entire job is catching work that leaves its expected paths** —
`workspace-boundary.md`-style rules must load regardless of where work is happening, or they can
never fire for the one case they exist to catch.

## Before merging or renaming a rule file

Check who cites it by exact filename first: `rg -l --hidden -g '!.git' "<filename>"` across
`.claude/agents/` and `tests/`. A rule cited by
name from an agent brief, a hook, or a test comment is not safe to merge or rename without
updating every citation — and the token savings from merging small files are usually smaller than
that blast radius. Prefer leaving topically-similar-but-separately-cited files as they are.

## A new rule names what it replaces

A new rule file, agent, gate, or required review step SHALL name the rule it replaces (the same change deletes or narrows that rule), or SHALL carry an "Added: <yyyy-mm-dd>" stamp on its section.
A later material edit adds or replaces an "Updated: <yyyy-mm-dd>" stamp.
A rule that does neither does not land.
Review each section whose newest Added or Updated date is more than 3 months old: keep it, narrow it, or delete it, and add "Updated: <yyyy-mm-dd>" to a kept section.

Why: every incident or review adds process, and nothing removes it, so process only grows.

## Anti-patterns

- **A new paragraph appended every time a mistake recurs.** This is the pattern that prompted this
  rule — see the worked example below.
- **Restating the same fact in prose across multiple files.** Already banned for counts by
  [repo-conventions.md](repo-conventions.md)'s "measured fact belongs in one place"; the same
  applies to a restated incident, a restated rationale, a restated example.
- **Narrating the document's own edit history inline** — governed already by
  [no-inlined-change-history.md](no-inlined-change-history.md); don't duplicate its content here,
  link to it.
- **Hedge words on a SHALL/MUST**: "generally," "in most cases," "usually," "consider," "try to."
  A directive is unambiguous or it isn't a directive.
- **An "evidence" section whose only job is convincing the reader the rule is real.** A citation
  does that job in one line; a paragraph doesn't do it better, it just costs more to read.

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

The directive is identical in both. The good version is shorter and adds an occurrence in one row.

## Enforcement

Behavioral, same model as [no-inlined-change-history.md](no-inlined-change-history.md): no regex
reliably tells a load-bearing "Why" sentence from padding. Applied at the moment a document is
written or edited — the keep-or-cut test is: *would a reader do anything differently because of this
sentence?* If not, cut it. Checked at whatever review already runs over the document, or in a human pass, not by a dedicated gate.

## Scope

This rule applies to `.claude/rules/**`, `.claude/skills/**`, and `.claude/agents/**` in this repository.
Ledgers and retrospectives hold historical narratives.
See [no-inlined-change-history.md](no-inlined-change-history.md) for the history rule.
