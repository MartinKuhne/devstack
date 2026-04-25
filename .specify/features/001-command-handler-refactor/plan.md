# Implementation Plan: Command Handler Refactoring

**Branch**: `feature/1-graphql-command-handlers` | **Date**: Apr 24 2026 | **Spec**: [.specify/features/001-command-handler-refactor/spec.md](./spec.md)
**Input**: Feature specification from `.specify/features/001-command-handler-refactor/spec.md`

## Summary

Refactor the GraphQL server mutations to use command handlers for all write operations, maintain direct database access for queries, and adapt the MCP server to use the same command handlers for updates while keeping direct database access for reads. The implementation uses the existing generic `ICommandHandler<TResult, TCommand>` interface pattern.

## Technical Context

**Language/Version**: C# .NET 10  
**Primary Dependencies**: Entity Framework Core, GraphQL.NET, ModelContextProtocol  
**Storage**: PostgreSQL via Entity Framework Core  
**Testing**: xUnit for unit tests, integration tests against real database  
**Target Platform**: Linux container  
**Project Type**: GraphQL API server + MCP server  
**Performance Goals**: Maintain current response times, zero regression in test suite  
**Constraints**: Backward compatible GraphQL schema, no breaking changes to MCP tools  
**Scale/Scope**: Refactor existing mutations (Project, Deliverable, AgentTask, LargeLanguageModel) and MCP tools  

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] Open Source First: All code is developed as open source by default
- [x] Infrastructure as Code: Infrastructure already provisioned through declarative code
- [x] Observability-Driven Development: Services already emit structured logs, traces, and metrics
- [x] Uncompromising Quality: Code must remain readable, testable, and maintainable
- [x] Progress Over Perfection: Work broken into independently testable increments (per entity type)

## Project Structure

### Documentation (this feature)

```text
.specify/features/001-command-handler-refactor/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── checklists/
    └── requirements.md  # Specification quality checklist
```

### Source Code (existing structure)

```text
src/Server/
├── DevStack.Api/                    # GraphQL API layer
│   └── GraphQL/
│       ├── Mutation.cs              # Refactor: delegate to command handlers
│       └── Query.cs                 # Keep: direct database access
├── DevStack.Application/            # Command handlers and commands
│   ├── Projects/
│   │   ├── Commands/
│   │   └── Queries/
│   ├── Deliverables/
│   │   ├── Commands/
│   │   └── Queries/
│   ├── AgentTasks/
│   │   ├── Commands/
│   │   └── Queries/
│   └── ICommandHandler.cs           # Existing: generic interface
├── DevStack.Infrastructure/         # Handler implementations
│   ├── Projects/
│   │   └── ProjectHandlers.cs       # Update to generic interface
│   ├── Deliverables/
│   │   └── DeliverableHandlers.cs   # Update to generic interface
│   └── AgentTasks/
│       └── AgentTaskHandlers.cs     # Update to generic interface
├── DevStack.Mcp/                    # MCP server
│   └── Tools/
│       ├── ProjectTools.cs          # Update writes to use commands
│       ├── DeliverableTools.cs      # Update writes to use commands
│       └── TaskTools.cs             # Update writes to use commands
├── DevStack.Persistence/            # Database context
│   └── DevStackDbContext.cs        # Keep unchanged
└── DevStack.Domain/                 # Domain entities
    └── Entities/                    # Keep unchanged
```

**Structure Decision**: Existing architecture preserved. Only refactoring mutation resolvers to delegate to command handlers, updating handler implementations to use generic interface, and adapting MCP tools for writes.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations. All constitution principles maintained.

## Implementation Phases

### Phase 0: Research & Analysis

**Goal**: Understand current implementation and identify all mutations needing refactoring

1. Analyze Mutation.cs to identify all mutations using direct database access
2. Review existing command handlers and their interface patterns
3. Identify missing command classes for Deliverable and AgentTask operations
4. Document MCP server tools that perform write operations
5. Create research.md with findings

### Phase 1: Design & Contracts

**Goal**: Define the refactoring approach and update documentation

1. Create data-model.md documenting entities affected by the refactoring
2. Update command handler interface usage documentation
3. Create contracts documenting the command handler pattern
4. Update quickstart.md with refactoring guidelines
5. Create missing command classes for Deliverable and AgentTask operations
6. Update existing handlers to use `ICommandHandler<TResult, TCommand>` interface

### Phase 2: Implementation Planning

**Goal**: Create detailed task breakdown for execution

1. Generate tasks.md with all implementation tasks
2. Prioritize tasks by entity type (Projects → Deliverables → AgentTasks → LLMs)
3. Define testing strategy for each refactored mutation
4. Create migration checklist for verifying refactoring completeness

## Quality Gates

All quality gates must pass before marking complete:

- [ ] Build succeeds with no warnings
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] GraphQL mutations delegate to command handlers (verified by code review)
- [ ] GraphQL queries use direct database access (verified by code review)
- [ ] MCP server updates use command handlers (verified by code review)
- [ ] MCP server reads use direct database access (verified by code review)
