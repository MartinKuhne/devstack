using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace DevStack.Infrastructure.Tasks;

public record CreateTaskCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    string? Deliverable,
    string? AcceptanceCriteria,
    string? Risks,
    string? Result,
    string? RequiredFollowUps,
    int ComplexityRating,
    FeatureStatus Status,
    Guid? ParentItemId);

public record UpdateTaskCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? Deliverable,
    string? AcceptanceCriteria,
    string? Risks,
    string? Result,
    string? RequiredFollowUps,
    int? ComplexityRating);

public record TransitionTaskStatusCommand(
    Guid Id,
    FeatureStatus TargetStatus,
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

        var item = new Item
        {
            ProjectId = request.ProjectId,
            ItemType = ItemSubtype.Task,
            Title = request.Title,
            Description = request.Description,
            Deliverable = request.Deliverable,
            AcceptanceCriteria = request.AcceptanceCriteria,
            Risks = request.Risks,
            Result = request.Result,
            ComplexityRating = request.ComplexityRating,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return item.Id;
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
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

        if (!string.IsNullOrEmpty(request.Title)) item.Title = request.Title;
        if (request.Description is not null) item.Description = request.Description;
        if (request.Deliverable is not null) item.Deliverable = request.Deliverable;
        if (request.AcceptanceCriteria is not null) item.AcceptanceCriteria = request.AcceptanceCriteria;
        if (request.Risks is not null) item.Risks = request.Risks;
        if (request.Result is not null) item.Result = request.Result;
        if (request.ComplexityRating.HasValue)
        {
            if (request.ComplexityRating.Value < 1 || request.ComplexityRating.Value > 10)
                throw new ArgumentException("ComplexityRating must be between 1 and 10");
            item.ComplexityRating = request.ComplexityRating.Value;
        }

        item.UpdatedAt = DateTime.UtcNow;

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
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

        if (item.ItemType != ItemSubtype.Task)
            throw new InvalidOperationException($"Item with ID {request.Id} is not a Task.");

        item.Status = request.TargetStatus;
        item.UpdatedAt = DateTime.UtcNow;

        _dbContext.Items.Update(item);

        _dbContext.AuditEvents.Add(new AuditEvent
        {
            EntityType = "Task",
            EntityId = item.Id,
            EventType = "StatusChanged",
            OldValue = item.Status.ToString(),
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
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

        _dbContext.Items.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}