# AGENTS.md

This repository uses OpenSpec for spec-driven development. Follow these rules:

- [.claude/rules/openspec-workflow.md](.claude/rules/openspec-workflow.md)
- [.claude/rules/openspec-validation-warnings.md](.claude/rules/openspec-validation-warnings.md)

## Where instructions live

An instruction is any text that tells an agent what to do.
Its audience is the set of roles that need it.
Its trigger is the moment it must already be in context.

1. Reliable delivery. Every instruction reaches its whole audience. The harness always delivers some text: this file, auto-loaded rules, and the body of an agent definition. That text carries the instruction, or an explicit startup step that loads it. A startup load that the model performs must be detectable. The role confirms it. The party that depends on it checks the confirmation.
2. One home. Each instruction lives in exactly one file. Other files reference that file. They never restate, summarize, or paraphrase it. Do not restate what the harness already delivers, such as agent descriptions.
3. Narrowest audience, before its trigger. Load an instruction only for its audience, as late as possible but always before the trigger. An instruction that exists only in troubleshooting notes fails this rule.

When two of these conflict, the lower number wins, and the file that makes the trade-off states it.
