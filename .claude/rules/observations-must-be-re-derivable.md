---
paths:
  - ".scratch/**"
---

# An observation carries its re-derivation, or it carries a warning label

## Rule

Two obligations, and the second exists because the first cannot always be met.

1. **Record only what you verified yourself.** An observation someone else made — a dispatched
   agent, a review comment, a prior session, a document — is *their* observation until you re-run it.
   Relaying it under your own voice converts a claim into a fact, and nothing downstream can tell
   which it was.
2. **Every recorded observation names the command that re-creates it.** Not "I checked"; the
   invocation, copy-pasteable. Where you cannot produce that command, the observation is
   **labeled** rather than dropped, per the next section.

This is the same principle as [repo-conventions.md](repo-conventions.md)'s "Never write a bare
count", generalized from numbers to observations. A count is one kind of observation that goes stale;
a behavior, an error code, a tool response, and a failure mode are the others, and they go stale the
same way and read exactly as confidently.

## The label

Where you cannot give a command that re-creates the observation, mark it:

> `NOT RE-DERIVED (<reason>)`

The reason is mandatory and is one of two shapes:

- `NOT RE-DERIVED (second-hand — <who observed it>, not me)`
- `NOT RE-DERIVED (conditions gone — <what changed>)`

**Why not "weak", "unreliable", or "indecisive".** Those describe *confidence*, and confidence is the
wrong axis. An un-re-derived observation may be perfectly true, and a re-derivable one may be wrong.
Labeling on confidence invites two opposite errors: discounting a correct finding nobody has re-run,
and trusting an incorrect one merely because someone could.

**Why not "unrepeatable".** That asserts impossibility, and the honest claim is almost always weaker:
nobody *has* re-run this, not nobody *can*. A degraded workspace can be re-induced; an agent's query
can be re-issued; a deleted file is one `git show` away. Overclaiming impossibility also removes the
incentive to go and get the evidence, which is the opposite of what the label is for.

`NOT RE-DERIVED` states what happened rather than what is possible.

A labeled observation is still worth recording, and the label is an invitation rather than a verdict:
the next reader who needs the claim to be true knows precisely what is missing. What is forbidden is
an un-re-derived observation wearing the grammar of a measurement.

## What counts as a re-derivation

The test is whether a reader can run one thing and get the same answer:

| Form | Sufficient? |
|---|---|
| an `rg` invocation with its flags, or an exact file and line range | yes |
| a `dotnet run --file scripts/<name>.cs` run | yes |
| a named test, filter included | yes |
| "verified against source" / "confirmed semantically" / "I checked" | **no** — names no command |
| a conclusion whose inputs no longer exist | **no** — label it |

**A re-derivation that no longer returns the recorded answer is a finding, not a formatting problem.**
That is the whole point of writing it down: the command is what lets the next reader discover the
observation went stale, instead of inheriting it as fact.

## Why

A relayed observation is indistinguishable in the record from a personally verified one — both read
as confident fact — so a fabricated measurement, a wrongly retracted true statement, and a relayed
hypothesis reported as a confirmed product defect have all reached this repository's artifacts
stated as fact, none re-derivable, each by the driver relaying another party's claim under its own
voice. A retraction carries the same obligation as an assertion: relaying an agent's negative is the
same act as relaying its positive, and feels more like diligence.

## Boundaries

- **This does not forbid citing other people's work.** It requires attribution and a label:
  "the review report records X (`.scratch/<task>/report.md`)" is a citation, and citations are
  encouraged. What is forbidden is restating someone else's observation as your own reading.
- **It does not demand you re-run everything.** Where a cited artifact already carries the command
  and the output, cite it rather than re-deriving — that is the count rule's "prefer citing the
  source over restating it", applied here.
- **A measurement you performed is not a claim about current state**, and the distinction survives
  here. Report a test total or a search's hit count with its command and its date; a reader can then
  tell evidence-about-your-run from an assertion about the tree.
- **The label is not an escape hatch for laziness.** Reach for it when you genuinely cannot produce
  the command, not when producing it would be inconvenient. Those two look identical in the record,
  which is why the reason is mandatory and has to name what is missing. Where re-derivation is merely
  **expensive** rather than unavailable — a probe to write, a fixture to build, a long run — say what
  it would cost, so a reader can decide to pay it instead of inheriting your uncertainty.

## Enforcement

Behavioral, and reviewable at record time rather than at observation time.
No hook can tell a verified observation from a relayed one, but any reader can see
whether the sentence names a command.

It is checked where observations get written down: an agent's findings entry (whose `method:` and
`basis:` lines in [agent-findings-ledger.md](agent-findings-ledger.md) are this rule's per-claim
form), a bug report's reproduction, a review finding, a friction-log entry, and a commit message.

A recorded observation with neither a re-derivation nor a `NOT RE-DERIVED` label is a review finding,
at whatever severity its load-bearing-ness warrants.
