using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using System.Threading.Tasks;
using TaskStatus = DevStack.Domain.Enums.TaskStatus;

namespace DevStack.Infrastructure.Tasks;

public record CreateTaskCommand(
    Guid FeatureId,
    string Title,
    string? Deliverable,
    string? AcceptanceCriteria,
    string? Risks,
    string? Result,
    string? RequiredFollowUps,
    int ComplexityRating);

public record UpdateTaskCommand(
    Guid Id,
    string? Title,
    string? Deliverable,
    string? AcceptanceCriteria,
    string? Risks,
    string? Result,
    string? RequiredFollowUps,
    int? ComplexityRating);

public record TransitionTaskStatusCommand(
    Guid Id,
    TaskStatus TargetStatus,
    string Actor);

public record DeleteTaskCommand(Guid Id);

public interface ICreateTaskHandler : DevStack.Application.ICommandHandler<Guid, CreateTaskCommand>
{
}

public interface IUpdateTaskHandler : DevStack.Application.ICommandHandler<UpdateTaskCommand>
{
}

public interface ITransitionTaskStatusHandler : DevStack.Application.ICommandHandler<TransitionTaskStatusCommand>
{
}

public interface IDeleteTaskHandler : DevStack.Application.ICommandHandler<DeleteTaskCommand>
{
}

public class CreateTaskHandler : ICreateTaskHandler
{
    private readonly DevStackDbContext _dbContext;

    public CreateTaskHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (request.ComplexityRating < 1 || request.ComplexityRating > 10)
            throw new ArgumentException("ComplexityRating must be between 1 and 10");

        var task = new global::DevStack.Domain.Entities.AgentTask
        {
            FeatureId = request.FeatureId,
            Title = request.Title,
            Deliverable = request.Deliverable,
            AcceptanceCriteria = request.AcceptanceCriteria,
            Risks = request.Risks,
            Result = request.Result,
            RequiredFollowUps = request.RequiredFollowUps,
            ComplexityRating = request.ComplexityRating,
            Status = TaskStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}

public class UpdateTaskHandler : IUpdateTaskHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateTaskHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks.FindAsync([request.Id], cancellationToken);
        if (task == null)
            throw new InvalidOperationException($"Task with ID {request.Id} not found.");

        if (!string.IsNullOrEmpty(request.Title)) task.Title = request.Title;
        if (request.Deliverable is not null) task.Deliverable = request.Deliverable;
        if (request.AcceptanceCriteria is not null) task.AcceptanceCriteria = request.AcceptanceCriteria;
        if (request.Risks is not null) task.Risks = request.Risks;
        if (request.Result is not null) task.Result = request.Result;
        if (request.RequiredFollowUps is not null) task.RequiredFollowUps = request.RequiredFollowUps;
        if (request.ComplexityRating.HasValue)
        {
            if (request.ComplexityRating.Value < 1 || request.ComplexityRating.Value > 10)
                throw new ArgumentException("ComplexityRating must be between 1 and 10");
            task.ComplexityRating = request.ComplexityRating.Value;
        }

        task.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class TransitionTaskStatusHandler : ITransitionTaskStatusHandler
{
    private readonly DevStackDbContext _dbContext;

    public TransitionTaskStatusHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(TransitionTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks.FindAsync([request.Id], cancellationToken);
        if (task == null)
            throw new InvalidOperationException($"Task with ID {request.Id} not found.");

        // Simple status transition without complex validation for now
        // A proper TaskStatusTransitionService should be created for full validation
        task.Status = request.TargetStatus;
        task.UpdatedAt = DateTime.UtcNow;

        _dbContext.Tasks.Update(task);

        _dbContext.AuditEvents.Add(new AuditEvent
        {
            EntityType = "Task",
            EntityId = task.Id,
            EventType = "StatusChanged",
            OldValue = task.Status.ToString(),
            NewValue = request.TargetStatus.ToString(),
            Actor = request.Actor,
            OccurredAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteTaskHandler : IDeleteTaskHandler
{
    private readonly DevStackDbContext _dbContext;

    public DeleteTaskHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks.FindAsync([request.Id], cancellationToken);
        if (task == null)
            throw new InvalidOperationException($"Task with ID {request.Id} not found.");

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}