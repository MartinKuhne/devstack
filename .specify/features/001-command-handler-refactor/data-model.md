# Data Model: Command Handler Refactoring

**Date**: Apr 24 2026

## Entities

This refactoring does not change the data model. The following entities are affected by the command handler pattern:

### Project

**Purpose**: Represents a development project in DevStack

**Fields**:
- `Id` (Guid) - Primary key
- `Name` (string) - Project name (max 200 characters)
- `Description` (string?) - Optional description
- `Repository` (string) - Git repository URL

**Relationships**:
- Has many Deliverables
- Has many AgentTasks

**Operations**:
- Create → `CreateProjectCommand` → `CreateProjectHandler`
- Update → `UpdateProjectCommand` → `UpdateProjectHandler`
- Delete → `DeleteProjectCommand` → `DeleteProjectHandler`

### Deliverable

**Purpose**: Represents a work item or task within a project

**Fields**:
- `Id` (Guid) - Primary key
- `ProjectId` (Guid) - Foreign key to Project
- `Title` (string) - Deliverable title
- `Type` (DeliverableType) - Type of deliverable
- `Description` (string) - Detailed description
- `Status` (DeliverableStatus) - Current status
- `AcceptanceCriteria` (string?) - Acceptance criteria
- `ExecutionPlan` (string?) - Execution plan
- `SecurityImpact` (string?) - Security impact assessment
- `PerformanceImpact` (string?) - Performance impact assessment
- `TestPlan` (string?) - Test plan
- `DeploymentPlan` (string?) - Deployment plan
- `AgentFeedback` (string?) - Feedback from agent
- `Blocking` (string?) - Blocking issues

**Relationships**:
- Belongs to Project
- Has many AgentTasks

**Operations**:
- Create → `CreateDeliverableCommand` → `CreateDeliverableHandler`
- Update → `UpdateDeliverableCommand` → `UpdateDeliverableHandler`
- UpdateStatus → `UpdateDeliverableStatusCommand` → `UpdateDeliverableStatusHandler`
- Delete → `DeleteDeliverableCommand` → `DeleteDeliverableHandler`

### AgentTask

**Purpose**: Represents an agent task associated with a deliverable

**Fields**:
- `Id` (Guid) - Primary key
- `ProjectId` (Guid) - Foreign key to Project
- `DeliverableId` (Guid) - Foreign key to Deliverable
- `Title` (string) - Task title
- `Description` (string) - Task description
- `Status` (AgentTaskStatus) - Current status
- `ComplexityRating` (int) - Complexity rating (1-10)
- `DependsOnAgentTaskId` (Guid?) - Dependency on other task
- `Result` (string?) - Task result
- `Errors` (string?) - Error messages
- `CommitHash` (string?) - Associated commit
- `PromptTokens` (int?) - Token usage
- `CompletionTokens` (int?) - Token usage
- `ExecutionDurationInSeconds` (int?) - Execution time
- `Agent` (string?) - Agent that executed

**Relationships**:
- Belongs to Project
- Belongs to Deliverable
- Optional dependency on another AgentTask

**Operations**:
- Create → `CreateAgentTaskCommand` → `CreateAgentTaskHandler`
- Update → `UpdateAgentTaskCommand` → `UpdateAgentTaskHandler`
- UpdateStatus → `UpdateAgentTaskStatusCommand` → `UpdateAgentTaskStatusHandler`
- Delete → `DeleteAgentTaskCommand` → `DeleteAgentTaskHandler`

### LargeLanguageModel

**Purpose**: Represents a configured LLM endpoint

**Fields**:
- `Id` (Guid) - Primary key
- `Url` (string) - LLM endpoint URL
- `Model` (string) - Model name
- `ModelAlias` (string?) - Alternative name
- `ApiKey` (string) - API key (encrypted)
- `MaxComplexity` (int) - Maximum complexity rating
- `MaxConcurrency` (int) - Maximum concurrent requests

**Relationships**:
- None (standalone configuration)

**Operations**:
- Create → `CreateLargeLanguageModelCommand` → `CreateLargeLanguageModelHandler`
- Update → `UpdateLargeLanguageModelCommand` → `UpdateLargeLanguageModelHandler`
- Delete → `DeleteLargeLanguageModelCommand` → `DeleteLargeLanguageModelHandler`

## Enums

### DeliverableStatus

- Draft
- Planning
- InProgress
- NeedsReview
- Done
- Failed
- Rejected

### AgentTaskStatus

- Ready
- InProgress
- Done
- Failed
- Rejected

### DeliverableType

- Feature
- BugFix
- Improvement
- Documentation
- Refactoring
- Test
- Infrastructure

## Command Handler Pattern

### Interface Definition

```csharp
// With return value
public interface ICommandHandler<TReturn, TCommand>
{
    Task<TReturn> Handle(TCommand command, CancellationToken cancellationToken = default);
}

// Without return value (void)
public interface ICommandHandler<TCommand>
{
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}
```

### Usage Examples

**Create command (returns ID)**:
```csharp
public class CreateProjectHandler : ICommandHandler<Guid, CreateProjectCommand>
{
    public async Task<Guid> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        // Create project and return ID
    }
}
```

**Update command (no return)**:
```csharp
public class UpdateProjectHandler : ICommandHandler<UpdateProjectCommand>
{
    public async Task Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        // Update project, no return value
    }
}
```

**Status update (returns new status)**:
```csharp
public class UpdateDeliverableStatusHandler : ICommandHandler<DeliverableStatus, UpdateDeliverableStatusCommand>
{
    public async Task<DeliverableStatus> Handle(UpdateDeliverableStatusCommand command, CancellationToken cancellationToken)
    {
        // Update status and return new status
    }
}
```

## Dependency Injection Registration

All command handlers must be registered in the DI container:

```csharp
// Projects
builder.Services.AddScoped<ICommandHandler<Guid, CreateProjectCommand>, CreateProjectHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateProjectCommand>, UpdateProjectHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteProjectCommand>, DeleteProjectHandler>();

// Deliverables
builder.Services.AddScoped<ICommandHandler<Guid, CreateDeliverableCommand>, CreateDeliverableHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateDeliverableCommand>, UpdateDeliverableHandler>();
builder.Services.AddScoped<ICommandHandler<DeliverableStatus, UpdateDeliverableStatusCommand>, UpdateDeliverableStatusHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteDeliverableCommand>, DeleteDeliverableHandler>();

// AgentTasks
builder.Services.AddScoped<ICommandHandler<Guid, CreateAgentTaskCommand>, CreateAgentTaskHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateAgentTaskCommand>, UpdateAgentTaskHandler>();
builder.Services.AddScoped<ICommandHandler<AgentTaskStatus, UpdateAgentTaskStatusCommand>, UpdateAgentTaskStatusHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteAgentTaskCommand>, DeleteAgentTaskHandler>();

// LargeLanguageModels
builder.Services.AddScoped<ICommandHandler<Guid, CreateLargeLanguageModelCommand>, CreateLargeLanguageModelHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateLargeLanguageModelCommand>, UpdateLargeLanguageModelHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteLargeLanguageModelCommand>, DeleteLargeLanguageModelHandler>();
```
