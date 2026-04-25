# Feature Specification: Command Handler Refactoring

**Feature Branch**: `feature/001-command-handler-refactor`  
**Created**: Apr 24 2026  
**Status**: Draft  
**Input**: User description: "refactor the graphql server in src/server to use command handlers for all mutations. Use direct database access for queries. command handlers must use a generic ICommandHandler<TResult, TCommand> interface. Then. adapt src/DevStack.MCP to use the same command handlers for updates only. Do not change the MCP server direct database access for reads"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - GraphQL mutations use command handlers (Priority: P1)

As a developer maintaining the GraphQL API, I want all mutations to use command handlers so that business logic is centralized, testable, and consistent across the application.

**Why this priority**: This is the core requirement that establishes the architectural pattern for all write operations. Without this, the refactoring cannot proceed.

**Independent Test**: Can be validated by inspecting Mutation.cs and confirming all mutation methods delegate to command handlers instead of directly modifying the database.

**Acceptance Scenarios**:

1. **Given** a GraphQL mutation for creating/updating/deleting entities, **When** the mutation is executed, **Then** it delegates to the appropriate command handler
2. **Given** a command handler, **When** it processes a command, **Then** it encapsulates all business logic and validation
3. **Given** a mutation that needs to return updated data, **When** the handler completes, **Then** the mutation queries the database directly to fetch and return the result

---

### User Story 2 - Queries use direct database access (Priority: P2)

As a developer maintaining the GraphQL API, I want queries to use direct database access so that read operations remain simple and performant without unnecessary abstraction layers.

**Why this priority**: Queries are read-only operations that don't require complex business logic orchestration. Direct database access is appropriate and keeps the code simple.

**Independent Test**: Can be validated by inspecting Query.cs and confirming all query methods use DbContext directly without command/query handlers.

**Acceptance Scenarios**:

1. **Given** a GraphQL query, **When** it is executed, **Then** it uses DbContext directly to fetch data
2. **Given** a query method, **When** it needs data, **Then** it constructs and executes EF Core queries directly

---

### User Story 3 - Command handlers use generic interface (Priority: P3)

As a developer, I want all command handlers to implement a generic ICommandHandler<TResult, TCommand> interface so that the pattern is consistent, type-safe, and easy to register in dependency injection.

**Why this priority**: The generic interface is the foundation of the command handler pattern and must be established before handlers can be implemented.

**Independent Test**: Can be validated by checking that all command handlers implement the generic interface and that the interface is properly registered in DI.

**Acceptance Scenarios**:

1. **Given** a command type, **When** a handler is implemented, **Then** it implements ICommandHandler<TResult, TCommand> where TResult is the return type
2. **Given** a void command, **When** a handler is implemented, **Then** it implements ICommandHandler<TCommand> (non-generic return)
3. **Given** the DI container, **When** handlers are registered, **Then** they are registered with their generic interface types

---

### User Story 4 - MCP server uses command handlers for updates (Priority: P4)

As a developer maintaining the MCP server, I want update operations to use the same command handlers as the GraphQL API so that business logic is shared and consistent across both interfaces.

**Why this priority**: This extends the command handler pattern to the MCP server, ensuring consistency. It depends on P1 being completed first.

**Independent Test**: Can be validated by inspecting MCP tool files and confirming update operations delegate to command handlers.

**Acceptance Scenarios**:

1. **Given** an MCP tool that updates data, **When** the tool is invoked, **Then** it uses command handlers for the update operation
2. **Given** an MCP tool that reads data, **When** the tool is invoked, **Then** it continues to use direct database access (unchanged)
3. **Given** a command handler used by both GraphQL and MCP, **When** either interface invokes it, **Then** the same business logic is executed

---

### Edge Cases

- What happens when a command handler throws an exception? → GraphQL mutations should handle exceptions appropriately (existing error handling)
- How does the system handle concurrent updates? → Command handlers should handle concurrency as they do currently
- What if a command handler is not registered? → Dependency injection will fail at startup (expected behavior)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST use command handlers for all GraphQL mutations
- **FR-002**: System MUST use direct database access (DbContext) for all GraphQL queries
- **FR-003**: Command handlers MUST implement ICommandHandler<TResult, TCommand> interface
- **FR-004**: Command handlers MUST encapsulate all business logic for mutations
- **FR-005**: GraphQL mutations MUST query database directly after handler execution to return updated data
- **FR-006**: MCP server update operations MUST use command handlers
- **FR-007**: MCP server read operations MUST continue using direct database access
- **FR-008**: All existing command handlers MUST be updated to use the generic interface pattern
- **FR-009**: Missing entity operations in mutations MUST be converted to use command handlers
- **FR-010**: Command handlers MUST be registered in dependency injection container

### Key Entities *(include if feature involves data)*

- **ICommandHandler<TResult, TCommand>**: Generic interface for command handlers with return value
- **ICommandHandler<TCommand>**: Generic interface for command handlers without return value
- **Command**: DTO or record representing an intent to perform an action
- **Handler**: Implementation that processes a command and performs business logic
- **Mutation**: GraphQL mutation resolver that delegates to command handlers
- **Query**: GraphQL query resolver that uses direct database access
- **MCP Tool**: MCP server tool that uses command handlers for updates

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of GraphQL mutations delegate to command handlers (zero direct database modifications in mutations)
- **SC-002**: 100% of GraphQL queries use direct database access (zero handler usage in queries)
- **SC-003**: All command handlers implement the generic ICommandHandler interface pattern
- **SC-004**: MCP server update operations use command handlers (create, update, status transition)
- **SC-005**: MCP server read operations remain unchanged (direct database access)
- **SC-006**: Application builds successfully with no compilation errors or warnings
- **SC-007**: All existing unit tests pass
- **SC-008**: All existing integration tests pass

## Assumptions

- Existing command classes (CreateProjectCommand, UpdateProjectCommand, etc.) can be reused without changes
- Existing command handler implementations can be adapted to the generic interface pattern
- The generic ICommandHandler<TResult, TCommand> interface already exists and can be used as-is
- DI registration for command handlers follows the existing pattern in the codebase
- MCP server already has some command handler usage that can be extended
- Database schema and entity models remain unchanged
- The refactoring maintains backward compatibility with existing GraphQL schema and MCP tools

</content>
<parameter=filePath>
C:\Users\mkuhn\src\devstack\.specify\features\001-command-handler-refactor\spec.md