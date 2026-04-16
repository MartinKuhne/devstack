# Development Guidelines

## Active Technologies
- C# 12, .NET 10 
- Use this https://wolverinefx.net/introduction/getting-started.html instead of MediatR

# Helpers
- Use the codebase-memory tool to index the codebase, find out about the architecture and how modules relate to each other
- Use the memory tool as a memory graph to create persistent memories
- Use the refactor tool to quickly make changes to existing code
- Use the context7 tool to research reference implementation to frequent code problems
- Use the nuget tool to manage .net libraries and dependencies
- Use the dotnet tool to analzye code and retrieve symbols
- Use the serena tool as an alternate way to index and research code
- Use the fetch tool for web access and retrieving data from URLs
- Use the git tool to create branches and pull request and to research code history
- Use the saga tool to manage todos and tasks
- Use the filesystem tool for efficient access to files
- Use the local rg binary in the path (ripgrep) for very fast searches

## Project Structure

The solution file is 'src/Server/DevStack.slnx'

```text
src/
```

## Commands

# Add commands for C# 12, .NET 10

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

# Knowledge


# Development process
- Create a detailed plan and decompose implementations steps into units of work that can be done by an AI agent in less than 20 minutes
- Specify dependencies, test impact, architecture changes, risk
- Specify complexity on a scale of 1 to 10
- Save the plan to the specs folder and create todos
- Ask for approval
- Execute the plan
- Check that all quality gates have passed
- Create a commit message with a summary of changes and commit

# Quality gates to commit changes

The following commands succeed without any errors

```
dotnet build src/server
dotnet test src/server
docker compose build
```
There are no build warnings or errors

# Code quality
- Once class per file
- All public and internal methods have a brief description
- Follow SOLID principles
- Use IReadOnlyCollection where possible
- Use Application, Domain and Infrastructure layers
- Use the [Microsoft naming conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)
- Use all [Microsoft coding](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Say it once
- Do not put Client IDs and any passwords and secrets in the code