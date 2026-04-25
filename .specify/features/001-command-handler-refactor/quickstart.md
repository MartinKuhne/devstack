# Quickstart: Command Handler Refactoring

**Date**: Apr 24 2026

## Overview

This guide explains how to add new command handlers and migrate existing operations to the command handler pattern.

## Adding a New Command Handler

### Step 1: Create the Command

Create a record in the appropriate namespace under `DevStack.Application`:

```csharp
namespace DevStack.Application.Projects.Commands;

public record CreateProjectCommand(
    string Name,
    string? Description,
    string? Repository);
```

### Step 2: Create the Handler

Create a handler class in `DevStack.Infrastructure`:

```csharp
namespace DevStack.Infrastructure.Projects;

public class CreateProjectHandler : ICommandHandler<Guid, CreateProjectCommand>
{
    private readonly DevStackDbContext _dbContext;

    public CreateProjectHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Name is required", nameof(command.Name));
        
        if (command.Name.Length > 200)
            throw new ArgumentException("Name must be 200 characters or less", nameof(command.Name));

        // Create entity
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            Repository = command.Repository
        };

        // Persist
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Return ID
        return project.Id;
    }
}
```

### Step 3: Register in DI

Add registration to `Program.cs` or the appropriate DI configuration:

```csharp
builder.Services.AddScoped<ICommandHandler<Guid, CreateProjectCommand>, CreateProjectHandler>();
```

### Step 4: Update Mutation/Tool

Delegate to the handler in your GraphQL mutation or MCP tool:

```csharp
// GraphQL Mutation
public async Task<Project?> CreateProjectAsync(
    [Service] DevStackDbContext dbContext,
    [Service] ICommandHandler<Guid, CreateProjectCommand> handler,
    CreateProjectInput input,
    CancellationToken cancellationToken)
{
    var id = await handler.Handle(new CreateProjectCommand(input.Name, input.Description, input.Repository), cancellationToken);
    return await dbContext.Projects.FindAsync(id, cancellationToken);
}
```

## Migrating Existing Operations

### Before (Direct Database Access)

```csharp
public async Task<Deliverable?> CreateDeliverableAsync(
    [Service] DevStackDbContext dbContext,
    CreateDeliverableInput input,
    CancellationToken cancellationToken)
{
    var deliverable = new Deliverable
    {
        ProjectId = input.ProjectId,
        Title = input.Title,
        // ... set other properties
    };

    dbContext.Deliverables.Add(deliverable);
    await dbContext.SaveChangesAsync(cancellationToken);
    return deliverable;
}
```

### After (Command Handler)

```csharp
// 1. Create command (if not exists)
public record CreateDeliverableCommand(
    Guid ProjectId,
    string Title,
    DeliverableType Type,
    string Description,
    // ... other fields
);

// 2. Create handler
public class CreateDeliverableHandler : ICommandHandler<Guid, CreateDeliverableCommand>
{
    public async Task<Guid> Handle(CreateDeliverableCommand command, CancellationToken cancellationToken)
    {
        var deliverable = new Deliverable
        {
            Id = Guid.NewGuid(),
            ProjectId = command.ProjectId,
            Title = command.Title,
            // ... set other properties
        };

        _dbContext.Deliverables.Add(deliverable);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return deliverable.Id;
    }
}

// 3. Register in DI
builder.Services.AddScoped<ICommandHandler<Guid, CreateDeliverableCommand>, CreateDeliverableHandler>();

// 4. Update mutation
public async Task<Deliverable?> CreateDeliverableAsync(
    [Service] DevStackDbContext dbContext,
    [Service] ICommandHandler<Guid, CreateDeliverableCommand> handler,
    CreateDeliverableInput input,
    CancellationToken cancellationToken)
{
    var id = await handler.Handle(new CreateDeliverableCommand(...), cancellationToken);
    return await dbContext.Deliverables.FindAsync(id, cancellationToken);
}
```

## Query Operations (No Change)

Queries continue to use direct database access:

```csharp
public async Task<Project?> GetProjectByIdAsync(
    [Service] DevStackDbContext dbContext,
    Guid id,
    CancellationToken cancellationToken)
{
    return await dbContext.Projects.FindAsync(id, cancellationToken);
}
```

## MCP Server Updates

### Read Operations (No Change)

```csharp
[McpServerTool(Name = "get_projects")]
public async Task<string> GetProjects(CancellationToken ct = default)
{
    var projects = await _dbContext.Projects.ToListAsync(ct);
    return FormatMarkdown(projects);
}
```

### Write Operations (Use Commands)

```csharp
[McpServerTool(Name = "update_project")]
public async Task<string> UpdateProject(
    [Description("Project ID")] Guid id,
    [Description("New name")] string? name,
    CancellationToken ct = default)
{
    await _handler.Handle(new UpdateProjectCommand(id, name, null, null), ct);
    return "Project updated";
}
```

## Common Patterns

### Create Command (Returns ID)

```csharp
public class CreateXHandler : ICommandHandler<Guid, CreateXCommand>
{
    public async Task<Guid> Handle(CreateXCommand command, CancellationToken cancellationToken)
    {
        // Create, save, return ID
    }
}
```

### Update Command (No Return)

```csharp
public class UpdateXHandler : ICommandHandler<UpdateXCommand>
{
    public async Task Handle(UpdateXCommand command, CancellationToken cancellationToken)
    {
        // Update, save, no return
    }
}
```

### Delete Command (No Return)

```csharp
public class DeleteXHandler : ICommandHandler<DeleteXCommand>
{
    public async Task Handle(DeleteXCommand command, CancellationToken cancellationToken)
    {
        // Delete, save, no return
    }
}
```

### Status Update (Returns Status)

```csharp
public class UpdateXStatusHandler : ICommandHandler<XStatus, UpdateXStatusCommand>
{
    public async Task<XStatus> Handle(UpdateXStatusCommand command, CancellationToken cancellationToken)
    {
        // Update status, return new status
    }
}
```

## Testing

### Unit Test Example

```csharp
public class CreateProjectHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesProject()
    {
        // Arrange
        var dbContext = new Mock<DevStackDbContext>();
        var handler = new CreateProjectHandler(dbContext.Object);
        var command = new CreateProjectCommand("Test", null, null);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().NotBe(Guid.Empty);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task Handle_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var handler = new CreateProjectHandler(new Mock<DevStackDbContext>().Object);
        var command = new CreateProjectCommand("", null, null);
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
    }
}
```

## Checklist

Before completing a handler migration:

- [ ] Command record created
- [ ] Handler class created
- [ ] Input validation implemented
- [ ] Exception handling appropriate
- [ ] CancellationToken used
- [ ] DI registration added
- [ ] Mutation/tool updated
- [ ] Unit tests added/updated
- [ ] Integration tests pass
- [ ] No breaking changes to API
