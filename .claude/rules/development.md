---
paths:
  - "src/**"
  - "tests/**"
---

# Development rules

These rules govern how this project is built. LLM agents and developers must follow them.

## Red–green–refactor (TDD)

- **Red**: Write the minimal test that expresses the intended behavior and watch it fail.
- **Green**: Write the minimal code that makes the test pass.
- **Iterate**: Add detail to the test step by step until it describes the full, desired behavior. Keep the tests passing.
- **Refactor**: When the behavior is complete, refactor for readability, maintainability, and stability. Do not change the behavior.

Do not add behavior without a failing test first.

## YAGNI / Keep It Simple

Do **not** add interfaces or abstract classes "for flexibility" until a concrete need exists
(for example two implementations, test doubles, or a clear extension point). Prefer simple, direct code.
