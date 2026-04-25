using DevStack.Application;
using DevStack.Application.AgentTasks;
using DevStack.Application.AgentTasks.Commands;
using DevStack.Application.AgentTasks.Queries;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Persistence;

namespace DevStack.Infrastructure.AgentTasks;

public class CreateAgentTaskHandler : ICreateAgentTaskHandler
{
    private readonly DevStackDbContext _dbContext;

    public CreateAgentTaskHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateAgentTaskCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ArgumentException("Title is required", nameof(command.Title));

        if (command.Title.Length > 200)
            throw new ArgumentException("Title must be 200 characters or less", nameof(command.Title));

        var deliverable = await _dbContext.Deliverables.FindAsync([command.DeliverableId], cancellationToken);
        if (deliverable == null)
            throw new InvalidOperationException($"Deliverable with ID {command.DeliverableId} not found.");

        if (command.DependsOnAgentTaskId.HasValue)
        {
            var dependencyTask = await _dbContext.AgentTasks.FindAsync([command.DependsOnAgentTaskId.Value], cancellationToken);
            if (dependencyTask == null)
                throw new InvalidOperationException($"Dependent AgentTask with ID {command.DependsOnAgentTaskId.Value} not found.");
        }

        var agentTask = new AgentTask
        {
            Id = Guid.NewGuid(),
            ProjectId = deliverable.ProjectId,
            DeliverableId = command.DeliverableId,
            Title = command.Title,
            Description = command.Description,
            ComplexityRating = command.ComplexityRating,
            DependsOnAgentTaskId = command.DependsOnAgentTaskId,
            Status = AgentTaskStatus.Ready
        };

        _dbContext.AgentTasks.Add(agentTask);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return agentTask.Id;
    }
}

public class UpdateAgentTaskHandler : IUpdateAgentTaskHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateAgentTaskHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateAgentTaskCommand command, CancellationToken cancellationToken)
    {
        var agentTask = await _dbContext.AgentTasks.FindAsync([command.Id], cancellationToken);
        if (agentTask == null)
            throw new InvalidOperationException($"AgentTask with ID {command.Id} not found.");

        if (!string.IsNullOrWhiteSpace(command.Title))
        {
            if (command.Title.Length > 200)
                throw new ArgumentException("Title must be 200 characters or less", nameof(command.Title));

            agentTask.Title = command.Title;
        }

        if (command.Description is not null) agentTask.Description = command.Description;
        if (command.Result is not null) agentTask.Result = command.Result;
        if (command.Errors is not null) agentTask.Errors = command.Errors;
        if (command.CommitHash is not null) agentTask.CommitHash = command.CommitHash;
        if (command.ComplexityRating.HasValue) agentTask.ComplexityRating = command.ComplexityRating.Value;
        if (command.PromptTokens.HasValue) agentTask.PromptTokens = command.PromptTokens;
        if (command.CompletionTokens.HasValue) agentTask.CompletionTokens = command.CompletionTokens;
        if (command.ExecutionDurationInSeconds.HasValue) agentTask.ExecutionDurationInSeconds = command.ExecutionDurationInSeconds;
        if (command.Agent is not null) agentTask.Agent = command.Agent;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class UpdateAgentTaskStatusHandler : IUpdateAgentTaskStatusHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateAgentTaskStatusHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateAgentTaskStatusCommand command, CancellationToken cancellationToken)
    {
        var agentTask = await _dbContext.AgentTasks.FindAsync([command.Id], cancellationToken);
        if (agentTask == null)
            throw new InvalidOperationException($"AgentTask with ID {command.Id} not found.");

        agentTask.Status = command.Status;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteAgentTaskHandler : IDeleteAgentTaskHandler
{
    private readonly DevStackDbContext _dbContext;

    public DeleteAgentTaskHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteAgentTaskCommand command, CancellationToken cancellationToken)
    {
        var agentTask = await _dbContext.AgentTasks.FindAsync([command.Id], cancellationToken);
        if (agentTask == null)
            throw new InvalidOperationException($"AgentTask with ID {command.Id} not found.");

        _dbContext.AgentTasks.Remove(agentTask);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class GetAgentTaskByIdHandler : DevStack.Application.AgentTasks.Queries.IGetAgentTaskByIdHandler
{
    private readonly DevStackDbContext _dbContext;

    public GetAgentTaskByIdHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AgentTask?> Handle(GetAgentTaskByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AgentTasks.FindAsync([query.Id], cancellationToken);
    }
}
