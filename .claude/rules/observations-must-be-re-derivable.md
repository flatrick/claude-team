---
paths:
  - ".scratch/**"
---

# An observation carries its re-derivation, or it carries a warning label

## Rule

This rule has two obligations. The second exists because you cannot always meet the first.

1. **Record only what you verified yourself.** An observation that someone else made is *their* observation
   until you re-run it. The other party can be a dispatched agent, a review comment, a prior session, or a document.
   If you relay it in your own voice, you convert a claim into a fact, and nothing downstream can tell
   which it was.
2. **Every recorded observation names the command that re-creates it.** Do not write "I checked".
   Write the invocation, copy-pasteable. If you cannot give that command, **label** the observation
   and do not drop it. The next section gives the label.

This is the same principle as "Never write a bare count" in [repo-conventions.md](repo-conventions.md),
applied to observations and not only to numbers. A count is one kind of observation that goes stale.
A behavior, an error code, a tool response, and a failure mode are other kinds. They go stale in the
same way, and they read just as confidently.

## The label

If you cannot give a command that re-creates the observation, mark it:

> `NOT RE-DERIVED (<reason>)`

The reason is mandatory. It has one of two shapes:

- `NOT RE-DERIVED (second-hand — <who observed it>, not me)`
- `NOT RE-DERIVED (conditions gone — <what changed>)`

**Why not "weak", "unreliable", or "indecisive".** These words describe *confidence*, and confidence is the
wrong axis. An observation that nobody re-derived can be true, and a re-derivable observation can be wrong.
A label based on confidence causes two opposite errors. One error is to discount a correct finding that nobody has re-run.
The other is to trust an incorrect finding only because someone could re-run it.

**Why not "unrepeatable".** This word asserts impossibility. The true claim is almost always weaker:
nobody *has* re-run the observation, not nobody *can*. Someone can re-induce a degraded workspace.
Someone can re-issue the query of an agent. A deleted file is one `git show` away. A claim of impossibility also removes the
incentive to get the evidence. The label exists to keep that incentive.

`NOT RE-DERIVED` states what happened, not what is possible.

A labeled observation is still worth recording. The label is an invitation, not a verdict.
The next reader who needs the claim to be true knows exactly what is missing. It is forbidden to write
an observation that nobody re-derived but that looks like a measurement.

## What counts as a re-derivation

The test is this: can a reader run one thing and get the same answer?

| Form | Sufficient? |
|---|---|
| an `rg` invocation with its flags, or an exact file and line range | yes |
| a `dotnet run --file scripts/<name>.cs` run | yes |
| a named test, filter included | yes |
| "verified against source" / "confirmed semantically" / "I checked" | **no** — names no command |
| a conclusion whose inputs no longer exist | **no** — label it |

**A re-derivation that no longer returns the recorded answer is a finding, not a formatting problem.**
This is the whole point of writing the command down. The command lets the next reader find that the
observation went stale. Without it, the reader takes the observation as fact.

## Why

In the record, a relayed observation looks the same as an observation that you verified. Both read
as confident fact. Three kinds of claim have reached the artifacts of this repository stated as fact,
and none was re-derivable: a fabricated measurement, a true statement that someone wrongly retracted,
and a relayed hypothesis that someone reported as a confirmed product defect. In each case the driver
relayed the claim of another party in its own voice. A retraction carries the same obligation as an assertion.
To relay the negative of an agent is the same act as to relay its positive, and it feels more like diligence.

## Boundaries

- **This does not forbid a citation of the work of other people.** It requires attribution and a label.
  "The review report records X (`.scratch/<task>/report.md`)" is a citation, and citations are
  encouraged. It is forbidden to restate the observation of someone else as your own reading.
- **It does not demand that you re-run everything.** If a cited artifact already has the command
  and the output, cite it and do not re-derive it. This is the "prefer citing the
  source over restating it" of the count rule, applied here.
- **A measurement that you performed is not a claim about the current state.** This distinction applies
  here too. Report a test total or the hit count of a search with its command and its date. A reader can then
  tell evidence about your run from an assertion about the tree.
- **The label is not a way to avoid work.** Use it when you really cannot produce
  the command, not when the command is inconvenient. These two cases look the same in the record.
  For this reason the reason is mandatory and must name what is missing. If a re-derivation is only
  **expensive** and not unavailable (a probe to write, a fixture to build, a long run), say what
  it would cost. A reader can then decide to pay the cost and does not inherit your uncertainty.

## Enforcement

Behavioral. A reviewer can check it when the observation is recorded, not when it is made.
No hook can tell a verified observation from a relayed observation, but any reader can see
whether the sentence names a command.

The check happens where people write observations down: the findings entry of an agent (its `method:` and
`basis:` lines in [agent-findings-ledger.md](agent-findings-ledger.md) are the per-claim
form of this rule), the reproduction in a bug report, a review finding, a friction-log entry, and a commit message.

A recorded observation with no re-derivation and no `NOT RE-DERIVED` label is a review finding.
Its severity depends on how much depends on it.
