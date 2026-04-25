# Research: Command Handler Refactoring

**Date**: Apr 24 2026  
**Purpose**: Document findings from analyzing current implementation and identify refactoring requirements

## Analysis Summary

### Current State

The GraphQL server currently has a mix of mutation implementations:

1. **Already using command handlers** (no changes needed):
   - `CreateProjectAsync` - uses `ICommandHandler<Guid, CreateProjectCommand>`
   - `UpdateProjectAsync` - uses `ICommandHandler<UpdateProjectCommand>`
   - `DeleteProjectAsync` - uses `ICommandHandler<DeleteProjectCommand>`
   - `CreateLargeLanguageModelAsync` - uses `ICreateLargeLanguageModelHandler`
   - `UpdateLargeLanguageModelAsync` - uses `IUpdateLargeLanguageModelHandler`

2. **Using direct database access** (needs refactoring):
   - `CreateDeliverableAsync` - directly creates Deliverable entity
   - `UpdateDeliverableAsync` - directly modifies Deliverable entity
   - `UpdateDeliverableStatusAsync` - directly updates Deliverable status
   - `DeleteDeliverableAsync` - directly removes Deliverable entity
   - `CreateAgentTaskAsync` - directly creates AgentTask entity
   - `UpdateAgentTaskAsync` - directly modifies AgentTask entity
   - `UpdateAgentTaskStatusAsync` - directly updates AgentTask status
   - `DeleteAgentTaskAsync` - directly removes AgentTask entity
   - `DeleteLargeLanguageModelAsync` - directly removes LLM entity
   - `CleanupTestDataAsync` - helper method for test cleanup
   - `CheckAndMarkDeliverableDoneAsync` - helper method for status transitions
   - `SetDeliverableToDoneAsync` - helper method for status transitions

### Existing Command Classes

Commands that already exist and can be reused:

**Projects:**
- `CreateProjectCommand` - Already exists
- `UpdateProjectCommand` - Already exists
- `DeleteProjectCommand` - Already exists

**Deliverables:**
- `CreateDeliverableCommand` - Already exists
- `UpdateDeliverableCommand` - Already exists
- `UpdateDeliverableStatusCommand` - Already exists
- `DeleteDeliverableCommand` - Already exists

**AgentTasks:**
- `CreateAgentTaskCommand` - Already exists
- `UpdateAgentTaskCommand` - Already exists
- `UpdateAgentTaskStatusCommand` - Already exists
- `DeleteAgentTaskCommand` - Already exists

**LargeLanguageModels:**
- `CreateLargeLanguageModelCommand` - Already exists
- `UpdateLargeLanguageModelCommand` - Already exists
- Delete command does not exist (needs creation)

### Existing Handler Interfaces

The codebase uses two patterns:

1. **Generic interface** (preferred, already defined):
   ```csharp
   public interface ICommandHandler<TReturn, TCommand>
   {
       Task<TReturn> Handle(TCommand command, CancellationToken cancellationToken = default);
   }
   
   public interface ICommandHandler<TCommand>
   {
       Task Handle(TCommand command, CancellationToken cancellationToken = default);
   }
   ```

2. **Custom interfaces** (needs migration):
   - `ICreateLargeLanguageModelHandler`
   - `IUpdateLargeLanguageModelHandler`
   - `IAgentTaskHandlers`
   - `IDeliverableHandlers`

### Handler Implementations

**Already using generic interface:**
- `UpdateProjectHandler : ICommandHandler<UpdateProjectCommand>`
- `DeleteProjectHandler : ICommandHandler<DeleteProjectCommand>`

**Using custom interfaces (needs migration):**
- `GetProjectByIdHandler : IGetProjectByIdHandler`
- `LargeLanguageModelHandlers` - custom interfaces
- `DeliverableHandlers` - custom interfaces
- `AgentTaskHandlers` - custom interfaces

## Refactoring Requirements

### Phase 1: Create Missing Commands

1. **DeleteLargeLanguageModelCommand** - Does not exist, needs to be created

### Phase 2: Create Missing Handlers

1. **Deliverable Handlers** - Need to create:
   - `CreateDeliverableHandler : ICommandHandler<Guid, CreateDeliverableCommand>`
   - `UpdateDeliverableHandler : ICommandHandler<UpdateDeliverableCommand>`
   - `UpdateDeliverableStatusHandler : ICommandHandler<DeliverableStatus, UpdateDeliverableStatusCommand>`
   - `DeleteDeliverableHandler : ICommandHandler<DeleteDeliverableCommand>`

2. **AgentTask Handlers** - Need to create:
   - `CreateAgentTaskHandler : ICommandHandler<Guid, CreateAgentTaskCommand>`
   - `UpdateAgentTaskHandler : ICommandHandler<UpdateAgentTaskCommand>`
   - `UpdateAgentTaskStatusHandler : ICommandHandler<AgentTaskStatus, UpdateAgentTaskStatusCommand>`
   - `DeleteAgentTaskHandler : ICommandHandler<DeleteAgentTaskCommand>`

3. **LargeLanguageModel Handlers** - Need to migrate:
   - `CreateLargeLanguageModelHandler : ICommandHandler<Guid, CreateLargeLanguageModelCommand>`
   - `UpdateLargeLanguageModelHandler : ICommandHandler<UpdateLargeLanguageModelCommand>`
   - `DeleteLargeLanguageModelHandler : ICommandHandler<DeleteLargeLanguageModelCommand>`

### Phase 3: Update Mutation Resolvers

Replace direct database access in mutations with command handler delegation:

1. `CreateDeliverableAsync` → Use `CreateDeliverableHandler`
2. `UpdateDeliverableAsync` → Use `UpdateDeliverableHandler`
3. `UpdateDeliverableStatusAsync` → Use `UpdateDeliverableStatusHandler`
4. `DeleteDeliverableAsync` → Use `DeleteDeliverableHandler`
5. `CreateAgentTaskAsync` → Use `CreateAgentTaskHandler`
6. `UpdateAgentTaskAsync` → Use `UpdateAgentTaskHandler`
7. `UpdateAgentTaskStatusAsync` → Use `UpdateAgentTaskStatusHandler`
8. `DeleteAgentTaskAsync` → Use `DeleteAgentTaskHandler`
9. `DeleteLargeLanguageModelAsync` → Use `DeleteLargeLanguageModelHandler`

### Phase 4: Helper Methods

The helper methods `CheckAndMarkDeliverableDoneAsync` and `SetDeliverableToDoneAsync` should be converted to command handlers:

1. Create `CheckAndMarkDeliverableDoneCommand` and handler
2. Create `SetDeliverableToDoneCommand` and handler (or incorporate into status update)

### Phase 5: MCP Server Updates

Update MCP tools to use command handlers for write operations:

1. **ProjectTools** - Add update/delete operations using command handlers
2. **DeliverableTools** - Add create/update/delete operations using command handlers
3. **TaskTools** - Add create/update/delete operations using command handlers

Read operations in MCP server remain unchanged (direct database access).

## Dependency Injection Registration

Command handlers need to be registered in the DI container. Current pattern:

```csharp
// In Program.cs or DI registration file
builder.Services.AddScoped<ICommandHandler<UpdateProjectCommand>, UpdateProjectHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteProjectCommand>, DeleteProjectHandler>();
```

New registrations needed for all new handlers.

## Testing Strategy

1. **Unit Tests** - Update existing unit tests to use command handlers
2. **Integration Tests** - Verify mutations work correctly with command handlers
3. **Regression Tests** - Ensure no breaking changes to GraphQL schema

## Risk Assessment

- **Low Risk**: Projects handlers already use the pattern
- **Medium Risk**: Deliverable and AgentTask handlers are new implementations
- **Low Risk**: LargeLanguageModel handlers exist, just need interface migration
- **Mitigation**: Comprehensive integration testing against real database

## Conclusion

The refactoring is straightforward as:
1. Command classes already exist for all operations
2. The generic interface is already defined
3. Some handlers already use the pattern successfully
4. The main work is creating missing handlers and updating mutation resolvers
