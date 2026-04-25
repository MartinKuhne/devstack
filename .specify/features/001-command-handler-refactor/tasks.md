# Implementation Tasks: Command Handler Refactoring

**Branch**: `feature/1-graphql-command-handlers`  
**Date**: Apr 24 2026  
**Plan**: [.specify/features/001-command-handler-refactor/plan.md](./plan.md)

## Task Summary

| Phase | Tasks | Description |
|-------|-------|-------------|
| Setup | 1 | Create missing command classes |
| Handlers | 2-9 | Implement command handlers for Deliverables, AgentTasks, LLMs |
| GraphQL | 10-13 | Update GraphQL mutations to use handlers |
| MCP | 14-16 | Update MCP tools to use handlers for writes |
| Testing | 17-19 | Update and run tests |
| Polish | 20-21 | Cleanup and verification |

## Phase 1: Setup

### Task 1: Create Missing Command Classes

**Files**:
- `src/Server/DevStack.Application/LargeLanguageModels/Commands/DeleteLargeLanguageModelCommand.cs` (already exists)
- `src/Server/DevStack.Application/AgentTasks/Queries/GetAgentTaskByIdQuery.cs` (created)

**Description**: Create the DeleteLargeLanguageModelCommand record that is missing from the codebase.

**Acceptance Criteria**:
- [x] Command record created with Id property
- [x] Follows existing command naming conventions
- [x] Located in correct namespace

---

## Phase 2: Create Command Handlers

### Task 2: CreateDeliverableHandler

**Files**:
- `src/Server/DevStack.Infrastructure/Deliverables/DeliverableHandlers.cs`

**Description**: Implement CreateDeliverableHandler that creates a new deliverable and returns its ID.

**Acceptance Criteria**:
- [x] Implements `ICommandHandler<Guid, CreateDeliverableCommand>`
- [x] Validates required fields (ProjectId, Title, Type, Description)
- [x] Creates Deliverable entity with proper default values
- [x] Saves to database and returns new ID
- [x] Handles validation errors with appropriate exceptions

### Task 3: UpdateDeliverableHandler

**Files**:
- `src/Server/DevStack.Infrastructure/Deliverables/DeliverableHandlers.cs`

**Description**: Implement UpdateDeliverableHandler that updates an existing deliverable.

**Acceptance Criteria**:
- [x] Implements `ICommandHandler<UpdateDeliverableCommand>`
- [x] Validates deliverable exists
- [x] Updates only non-null fields
- [x] Saves changes to database

### Task 4: UpdateDeliverableStatusHandler

**Files**:
- `src/Server/DevStack.Infrastructure/Deliverables/DeliverableHandlers.cs`

**Description**: Implement UpdateDeliverableStatusHandler that updates deliverable status.

**Acceptance Criteria**:
- [x] Implements `ICommandHandler<DeliverableStatus, UpdateDeliverableStatusCommand>`
- [x] Validates deliverable exists
- [x] Updates status only if different
- [x] Returns new status
- [x] Handles status transition logic

### Task 5: DeleteDeliverableHandler

**Files**:
- `src/Server/DevStack.Infrastructure/Deliverables/DeliverableHandlers.cs`

**Description**: Implement DeleteDeliverableHandler that deletes a deliverable.

**Acceptance Criteria**:
- [x] Implements `ICommandHandler<DeleteDeliverableCommand>`
- [x] Validates deliverable exists
- [x] Removes from database
- [x] Handles cascade delete for related AgentTasks

### Task 6: CreateAgentTaskHandler

**Files**:
- `src/Server/DevStack.Infrastructure/AgentTasks/AgentTaskHandlers.cs`

**Description**: Implement CreateAgentTaskHandler that creates a new agent task.

**Acceptance Criteria**:
- [x] Implements `ICommandHandler<Guid, CreateAgentTaskCommand>`
- [x] Validates deliverable exists
- [x] Sets default status to Ready
- [x] Validates dependency task if specified
- [x] Returns new task ID

### Task 7: UpdateAgentTaskHandler

**Files**:
- `src/Server/DevStack.Infrastructure/AgentTasks/AgentTaskHandlers.cs`

**Description**: Implement UpdateAgentTaskHandler that updates an existing agent task.

**Acceptance Criteria**:
- [x] Implements `ICommandHandler<UpdateAgentTaskCommand>`
- [x] Validates task exists
- [x] Updates only non-null fields
- [x] Saves changes to database

### Task 8: UpdateAgentTaskStatusHandler

**Files**:
- `src/Server/DevStack.Infrastructure/AgentTasks/AgentTaskHandlers.cs`

**Description**: Implement UpdateAgentTaskStatusHandler that updates task status and potentially marks deliverable as done.

**Acceptance Criteria**:
- [x] Implements `ICommandHandler<AgentTaskStatus, UpdateAgentTaskStatusCommand>`
- [x] Validates task exists
- [x] Updates status only if different
- [x] Checks and marks deliverable as done if all tasks complete
- [x] Returns new status

### Task 9: DeleteLargeLanguageModelHandler

**Files**:
- `src/Server/DevStack.Infrastructure/ModelConfigurations/LargeLanguageModelHandlers.cs`

**Description**: Implement DeleteLargeLanguageModelHandler that deletes an LLM configuration.

**Acceptance Criteria**:
- [x] Implements `ICommandHandler<DeleteLargeLanguageModelCommand>`
- [x] Validates model exists
- [x] Removes from database

---

## Phase 3: Update GraphQL Mutations

### Task 10: Update CreateDeliverableAsync

**Files**:
- `src/Server/DevStack.Api/GraphQL/Mutation.cs`

**Description**: Refactor CreateDeliverableAsync to use command handler instead of direct database access.

**Acceptance Criteria**:
- [ ] Injects `ICommandHandler<Guid, CreateDeliverableCommand>`
- [ ] Delegates creation to handler
- [ ] Fetches and returns created entity
- [ ] Removes direct database manipulation code

### Task 11: Update UpdateDeliverableAsync

**Files**:
- `src/Server/DevStack.Api/GraphQL/Mutation.cs`

**Description**: Refactor UpdateDeliverableAsync to use command handler.

**Acceptance Criteria**:
- [ ] Injects `ICommandHandler<UpdateDeliverableCommand>`
- [ ] Delegates update to handler
- [ ] Fetches and returns updated entity
- [ ] Removes direct database manipulation code

### Task 12: Update UpdateDeliverableStatusAsync

**Files**:
- `src/Server/DevStack.Api/GraphQL/Mutation.cs`

**Description**: Refactor UpdateDeliverableStatusAsync to use command handler.

**Acceptance Criteria**:
- [ ] Injects `ICommandHandler<DeliverableStatus, UpdateDeliverableStatusCommand>`
- [ ] Delegates status update to handler
- [ ] Returns status from handler
- [ ] Removes direct database manipulation code

### Task 13: Update Delete operations

**Files**:
- `src/Server/DevStack.Api/GraphQL/Mutation.cs`

**Description**: Refactor DeleteDeliverableAsync, DeleteAgentTaskAsync, DeleteLargeLanguageModelAsync to use command handlers.

**Acceptance Criteria**:
- [ ] Inject appropriate delete command handlers
- [ ] Delegate deletion to handlers
- [ ] Remove direct database manipulation code

---

## Phase 4: Update MCP Tools

### Task 14: Update ProjectTools for writes

**Files**:
- `src/Server/DevStack.Mcp/Tools/ProjectTools.cs`

**Description**: Add/update project update and delete operations to use command handlers.

**Acceptance Criteria**:
- [ ] Add UpdateProject tool using command handler
- [ ] Add DeleteProject tool using command handler
- [ ] Keep GetProjects and GetProjectById as direct database access
- [ ] Register handlers in DI

### Task 15: Update DeliverableTools

**Files**:
- `src/Server/DevStack.Mcp/Tools/DeliverableTools.cs`

**Description**: Add deliverable CRUD operations using command handlers for writes.

**Acceptance Criteria**:
- [ ] Add CreateDeliverable tool using command handler
- [ ] Add UpdateDeliverable tool using command handler
- [ ] Add DeleteDeliverable tool using command handler
- [ ] Keep read operations as direct database access

### Task 16: Update TaskTools

**Files**:
- `src/Server/DevStack.Mcp/Tools/TaskTools.cs`

**Description**: Add agent task CRUD operations using command handlers for writes.

**Acceptance Criteria**:
- [ ] Add CreateAgentTask tool using command handler
- [ ] Add UpdateAgentTask tool using command handler
- [ ] Add DeleteAgentTask tool using command handler
- [ ] Keep read operations as direct database access

---

## Phase 5: Testing

### Task 17: Update Unit Tests

**Files**:
- `src/Server/DevStack.Tests.Unit/`

**Description**: Update or create unit tests for new command handlers.

**Acceptance Criteria**:
- [ ] Tests for all new handlers
- [ ] Test validation logic
- [ ] Test success scenarios
- [ ] Test error scenarios
- [ ] All unit tests pass

### Task 18: Update Integration Tests

**Files**:
- `src/Server/DevStack.Tests.Integration.GraphQL/`
- `src/Server/DevStack.Tests.Integration.MCP/`

**Description**: Update integration tests to verify mutations use command handlers.

**Acceptance Criteria**:
- [ ] GraphQL mutation tests pass
- [ ] MCP tool tests pass
- [ ] No breaking changes to API
- [ ] All integration tests pass

### Task 19: Run Full Test Suite

**Command**: `dotnet test src/Server/DevStack.slnx`

**Description**: Run complete test suite to ensure no regressions.

**Acceptance Criteria**:
- [ ] All tests pass
- [ ] No new warnings
- [ ] Code coverage maintained

---

## Phase 6: Polish

### Task 20: Update DI Registration

**Files**:
- `src/Server/DevStack.Api/Program.cs` or `src/Server/DevStack.Infrastructure/DependencyInjection.cs`

**Description**: Ensure all new handlers are registered in DI container.

**Acceptance Criteria**:
- [ ] All handlers registered with correct interface types
- [ ] Registration follows consistent pattern
- [ ] Application starts without errors

### Task 21: Final Verification

**Description**: Verify all requirements from spec are met.

**Acceptance Criteria**:
- [ ] 100% of GraphQL mutations use command handlers
- [ ] 100% of GraphQL queries use direct database access
- [ ] MCP server updates use command handlers
- [ ] MCP server reads use direct database access
- [ ] Build succeeds with no warnings
- [ ] All tests pass
- [ ] Code is clean and follows coding standards

---

## Execution Notes

- **Sequential**: Tasks within each phase should be completed in order
- **Parallelizable**: Tasks in different entity groups (Deliverables, AgentTasks, LLMs) can be done in parallel
- **Critical Path**: Handlers must be created before mutations can be updated
- **Testing**: Run tests after each phase completes

## Quality Gates

Before marking complete:
- [ ] Build: `dotnet build src/Server/DevStack.slnx` - No errors or warnings
- [ ] Unit Tests: `dotnet test --filter "Category=Unit"` - All pass
- [ ] Integration Tests: `dotnet test --filter "Category=Integration"` - All pass
- [ ] Code Review: Verify all mutations delegate to handlers
- [ ] Test Coverage: Maintain or improve existing coverage
