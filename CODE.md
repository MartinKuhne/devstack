## Code Style

### Naming
**PascalCase:** classes, structs, records, interfaces, delegates, enums, namespaces, public/protected fields, properties, events, methods, local functions, constants (fields and local)

**camelCase:** method parameters, local variables, delegate instances

**Private/internal fields:** `_camelCase` (underscore prefix); static private/internal: `s_camelCase`; thread-static: `t_camelCase`

**Primary constructor parameters:** camelCase for `class`/`struct`; PascalCase for `record` (they become public properties)

**Type parameters:** single `T` when self-explanatory; descriptive names prefixed with `T` otherwise (e.g., `TSession`, `TInput`)

**Specific rules:**
- Interfaces: prefix with `I` → `IWorkerQueue`
- Attribute types: suffix with `Attribute` → `SerializableAttribute`
- Enums: singular noun for non-flags, plural noun for flags
- No double underscores (`__`) — reserved for compiler
- No single-letter names except simple loop counters
- No abbreviations unless widely accepted
- Namespaces: reverse-domain notation, meaningful and descriptive
- Avoid using `@` prefix except for interop with other languages

### Language
- Use language keywords over BCL types: `string` not `String`, `int` not `Int32`
- Use `var` only when the type is obvious from the right-hand side; explicit types in `foreach`, method returns, and non-obvious assignments
- Use `&&`/`||` not `&`/`|` for boolean logic
- Prefer `string` interpolation over concatenation; `StringBuilder` for loop concatenation
- Prefer raw string literals over escape sequences
- Use collection expressions `[...]` to initialize collections
- Use `Func<>` / `Action<>` instead of custom delegate types
- Use `using` declarations (no-brace form) instead of `try/finally` for `IDisposable`
- Use `async`/`await` for all I/O-bound operations; apply `ConfigureAwait` where appropriate
- Use `required` properties to enforce initialization instead of constructor overloads
- Catch specific exception types; never swallow or catch `Exception` without a filter
- Call static members via the class name, never via a derived class
- Use file-scoped namespace declarations (`namespace Foo.Bar;`)
- Place `using` directives outside the namespace declaration

### LINQ
- Use `var` for query variables and range variables
- Put `where` clauses before `orderby`/`select`
- Align query clauses under the `from` clause
- Use multiple `from` clauses instead of `join` for nested collections
- Use meaningful query variable names

### Formatting
- 4 spaces for indentation, no tabs
- One statement per line; one declaration per line
- Allman braces: opening and closing brace on their own line, aligned with indentation level
- Binary operators break before the operator when wrapping
- Blank line between method and property definitions
- Use parentheses to make precedence explicit in complex expressions

### Comments
- Single-line `//` comments; avoid `/* */`
- XML doc comments (`///`) on all public/internal members
- Comments on their own line, not trailing code
- Start with uppercase, end with period; one space after `//`

# Other notes

- Use IReadOnlyCollection where possible
- Use Application, Domain and Infrastructure layers
- Use the [Microsoft naming conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)
- Use all [Microsoft coding](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

## Immutability

This section defines immutable goals and functional code principles that an AI agent must follow when generating or modifying code. These rules are non‑negotiable constraints: they must be enforced automatically and reviewed in every change set.

### Immutable Goals
- Immutability of state — Domain state must be immutable by default; updates return new values rather than mutating in place.
- No hidden side effects — Functions must not perform I/O, global state mutation, or external calls unless explicitly declared.
- Explicit effect boundaries — All effectful operations must be isolated behind named adapters with typed contracts.
- Determinism — Given the same inputs and environment, functions must produce the same outputs; randomness and time are injected via parameters.

### Functional Principles
- Pure functions first — Business logic must be implemented as pure, referentially transparent functions.
- Honest interfaces — Signatures must declare all inputs and outputs; no implicit context or hidden dependencies.
- No implicit side effects — Any effect must be visible in the function’s type or signature.
- Idempotency — Public operations should be idempotent where feasible and provide clear error semantics.

### Quick Checklist
- Mutates shared state? — Reject unless wrapped in an approved adapter.
- Are side effects declared? — Reject if implicit.
- Is business logic pure and testable? — Prefer yes.
- Are adapters isolated and audited? — Must be yes.

## Refactoring

Refactoring is a disciplined, incremental process of applying small, behavior‑preserving transformations to improve design; an AI coding agent should treat it as a continuous, test‑backed activity that enforces modular, testable code and clear domain boundaries.

### Core principle
Refactoring preserves behavior while improving structure. Apply only small, reversible changes so tests can verify correctness after each step. This is the central definition and practice described by Martin Fowler. 

### High‑level development guidelines
- Always require tests before refactoring. The agent must add or verify automated tests that lock current behavior before any structural change. This enables safe, incremental transforms. 
- Prefer many tiny, behavior‑preserving steps. Each change should be “too small to be worth doing” on its own but cumulatively meaningful; commit frequently and keep diffs reviewable. 
- Follow the Rule of Three for duplication. Tolerate duplication until a pattern repeats; on the third occurrence, extract a shared abstraction. 
- Make domain boundaries explicit. Use modules, packages, or bounded contexts to separate domain logic; expose only typed adapters at effectful edges (I/O, DB, network).
- Favor pure functions and small interfaces inside domains. Keep side effects at the perimeter and inject effects via explicit adapters so core logic remains deterministic and testable.

### Practical refactoring actions the agent should perform
- Detect code smells (long methods, large classes, duplicated logic, feature envy) and prioritize low‑risk fixes. 
- Apply canonical refactorings: Extract Method, Extract Class, Move Method, Replace Conditional with Polymorphism, Introduce Parameter Object, Encapsulate Field. Each refactor must be accompanied by tests and a short rationale. 
- Enforce naming and intent: rename variables/functions to reflect domain concepts; prefer domain‑specific types over primitive bags.
- Modularize by domain: group code by bounded context, not by technical layer; create clear adapter interfaces for persistence and external services.

### Quick checklist for each refactor
- Tests exist and pass.
- Change is small and reversible.
- Behavior preserved by assertions.
- Domain boundary respected.
- Adapters isolate effects.
- Commit with rationale and tests.
