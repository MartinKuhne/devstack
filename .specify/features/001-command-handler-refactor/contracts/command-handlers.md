# Command Handler Contract

**Date**: Apr 24 2026  
**Purpose**: Define the contract for command handlers in DevStack

## Overview

All write operations in DevStack must use the command handler pattern. This ensures:

1. **Centralized business logic** - All rules for a specific operation in one place
2. **Testability** - Handlers can be tested independently
3. **Consistency** - Uniform pattern across all mutations and operations
4. **Separation of concerns** - GraphQL/MCP layers handle I/O, handlers handle business logic

## Interface Contract

### Generic Interface

```csharp
namespace DevStack.Application;

// Command handler that returns a value
public interface ICommandHandler<TReturn, TCommand>
{
    Task<TReturn> Handle(TCommand command, CancellationToken cancellationToken = default);
}

// Command handler that does not return a value
public interface ICommandHandler<TCommand>
{
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}
```

### Command Contract

Commands MUST:

1. Be immutable records
2. Contain all data needed to execute the operation
3. Have a clear, action-oriented name (e.g., `CreateProjectCommand`)
4. Validate input in the handler, not the command definition

Commands MUST NOT:

1. Contain behavior/logic (data carriers only)
2. Reference database entities directly
3. Have side effects

### Handler Contract

Handlers MUST:

1. Implement the appropriate `ICommandHandler` interface
2. Validate command parameters
3. Throw meaningful exceptions for validation failures
4. Use `CancellationToken` for cancellation support
5. Be registered in the DI container with the correct interface type

Handlers MUST NOT:

1. Access HTTP context or request-specific data
2. Throw generic exceptions (use specific exception types)
3. Perform I/O outside of data access (logging is OK)

## Registration Contract

All handlers MUST be registered in the DI container:

```csharp
// Pattern: Service registration
builder.Services.AddScoped<ICommandHandler<TCommand>, CommandHandler>();

// Example
builder.Services.AddScoped<ICommandHandler<Guid, CreateProjectCommand>, CreateProjectHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateProjectCommand>, UpdateProjectHandler>();
```

## Return Value Contract

### Create Commands

**Return**: `Guid` - The ID of the created entity

**Example**:
```csharp
public class CreateProjectHandler : ICommandHandler<Guid, CreateProjectCommand>
{
    public async Task<Guid> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        // Create and return new project ID
    }
}
```

### Update Commands

**Return**: `void` (use non-generic interface)

**Example**:
```csharp
public class UpdateProjectHandler : ICommandHandler<UpdateProjectCommand>
{
    public async Task Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        // Update project, no return value
    }
}
```

### Delete Commands

**Return**: `void` (use non-generic interface)

**Example**:
```csharp
public class DeleteProjectHandler : ICommandHandler<DeleteProjectCommand>
{
    public async Task Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        // Delete project, no return value
    }
}
```

### Status Update Commands

**Return**: The new status value

**Example**:
```csharp
public class UpdateProjectStatusHandler : ICommandHandler<ProjectStatus, UpdateProjectStatusCommand>
{
    public async Task<ProjectStatus> Handle(UpdateProjectStatusCommand command, CancellationToken cancellationToken)
    {
        // Update status and return new status
    }
}
```

## Exception Contract

Handlers MUST throw specific exceptions:

| Scenario | Exception Type | Example |
|----------|---------------|---------|
| Entity not found | `InvalidOperationException` | "Project with ID X not found" |
| Invalid input | `ArgumentException` | "Name must be 200 characters or less" |
| Validation failed | `InvalidOperationException` | "Project cannot be deleted with active tasks" |
| Concurrency conflict | `DbUpdateConcurrencyException` | (EF Core default) |

## Cancellation Contract

All handlers MUST support cancellation:

1. Accept `CancellationToken` parameter with default value
2. Pass token to all async operations
3. Respect cancellation requests promptly

```csharp
public async Task<Guid> Handle(CreateProjectCommand command, CancellationToken cancellationToken = default)
{
    // Pass token to all async calls
    await _dbContext.SaveChangesAsync(cancellationToken);
}
```

## GraphQL Integration Contract

Mutations MUST delegate to handlers:

```csharp
public async Task<Project?> CreateProjectAsync(
    [Service] DevStackDbContext dbContext,
    [Service] ICommandHandler<Guid, CreateProjectCommand> handler,
    CreateProjectInput input,
    CancellationToken cancellationToken)
{
    // 1. Execute command
    var id = await handler.Handle(new CreateProjectCommand(...), cancellationToken);
    
    // 2. Fetch created entity for return
    var project = await dbContext.Projects.FindAsync(id, cancellationToken);
    return project;
}
```

## MCP Integration Contract

MCP tools MUST use handlers for write operations:

```csharp
[McpServerTool(Name = "update_project")]
public async Task<string> UpdateProject(
    [Description("Project ID")] Guid id,
    [Description("New name")] string? name,
    CancellationToken ct = default)
{
    // Use command handler for update
    await _handler.Handle(new UpdateProjectCommand(id, name, null, null), ct);
    return "Project updated";
}
```

Read operations in MCP MUST continue using direct database access.

## Testing Contract

### Unit Tests

Test handlers in isolation:

```csharp
[Fact]
public async Task Handle_UpdatesProjectName()
{
    // Arrange
    var dbContext = new Mock<DevStackDbContext>();
    var handler = new UpdateProjectHandler(dbContext.Object);
    var command = new UpdateProjectCommand(id, "New Name", null, null);
    
    // Act
    await handler.Handle(command, CancellationToken.None);
    
    // Assert
    dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()));
}
```

### Integration Tests

Test mutations with real database:

```csharp
[Fact]
public async Task CreateProjectMutation_CreatesProject()
{
    // Arrange
    var input = new CreateProjectInput("Test", "https://github.com/test", "Test project");
    
    // Act
    var result = await mutation.CreateProjectAsync(dbContext, handler, input, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("Test");
}
```

## Migration Checklist

When migrating to command handlers:

- [ ] Command class exists (create if needed)
- [ ] Handler class implements `ICommandHandler`
- [ ] Handler validates input
- [ ] Handler throws appropriate exceptions
- [ ] Handler uses `CancellationToken`
- [ ] Handler registered in DI
- [ ] Mutation delegates to handler
- [ ] Unit tests updated
- [ ] Integration tests pass
- [ ] GraphQL schema unchanged
