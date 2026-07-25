using DevStack.Application.AgentTasks;
using DevStack.Application.AgentTasks.Commands;
using DevStack.Application.AgentTasks.Queries;

namespace DevStack.Infrastructure.AgentTasks;

public class CreateAgentTaskHandler : ICommandHandler<Guid, CreateAgentTaskCommand>
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

        var agentTask = new AgentTask(
            projectId: deliverable.ProjectId,
            deliverableId: command.DeliverableId,
            title: command.Title,
            description: command.Description,
            complexityRating: command.ComplexityRating,
            dependsOnAgentTaskId: command.DependsOnAgentTaskId,
            status: AgentTaskStatus.Ready);

        _dbContext.AgentTasks.Add(agentTask);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return agentTask.Id;
    }
}

public class UpdateAgentTaskHandler : ICommandHandler<UpdateAgentTaskCommand>
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

        agentTask.UpdateMetadata(
            title: command.Title,
            description: command.Description,
            result: command.Result,
            errors: command.Errors,
            commitHash: command.CommitHash,
            complexityRating: command.ComplexityRating,
            promptTokens: command.PromptTokens,
            completionTokens: command.CompletionTokens,
            executionDurationInSeconds: command.ExecutionDurationInSeconds,
            agent: command.Agent);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class UpdateAgentTaskStatusHandler : ICommandHandler<UpdateAgentTaskStatusCommand>
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

        agentTask.TransitionStatus(command.Status);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteAgentTaskHandler : ICommandHandler<DeleteAgentTaskCommand>
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

public class GetAgentTaskByIdHandler : ICommandHandler<AgentTask, GetAgentTaskByIdQuery>
{
    private readonly DevStackDbContext _dbContext;

    public GetAgentTaskByIdHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AgentTask> Handle(GetAgentTaskByIdQuery query, CancellationToken cancellationToken = default)
    {
        var agentTask = await _dbContext.AgentTasks.FindAsync([query.Id], cancellationToken);
        if (agentTask == null)
        {
            throw new InvalidOperationException($"AgentTask with ID {query.Id} not found.");
        }
        return agentTask;
    }
}
