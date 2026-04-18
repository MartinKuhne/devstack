using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace DevStack.Infrastructure.Defects;

public record CreateDefectCommand(
    Guid ProjectId,
    Guid? ParentFeatureId,
    string Title,
    string? Description,
    string? AcceptanceCriteria,
    string? Plan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? OpenQuestions,
    Severity? Severity,
    FeatureStatus? InitialStatus,
    Guid? DependsOnId);

public record UpdateDefectCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? AcceptanceCriteria,
    string? Plan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? OpenQuestions,
    Severity? Severity,
    string? RootCause,
    Guid? DependsOnId);

public record TransitionDefectStatusCommand(
    Guid Id,
    FeatureStatus TargetStatus,
    string Actor);

public record DeleteDefectCommand(Guid Id);

public interface ICreateDefectHandler : DevStack.Application.ICommandHandler<Guid, CreateDefectCommand>
{
}

public interface IUpdateDefectHandler : DevStack.Application.ICommandHandler<UpdateDefectCommand>
{
}

public interface ITransitionDefectStatusHandler : DevStack.Application.ICommandHandler<TransitionDefectStatusCommand>
{
}

public interface IDeleteDefectHandler : DevStack.Application.ICommandHandler<DeleteDefectCommand>
{
}

public class CreateDefectHandler : ICreateDefectHandler
{
    private readonly DevStackDbContext _dbContext;

    public CreateDefectHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task<Guid> Handle(CreateDefectCommand request, CancellationToken cancellationToken)
    {
         var item = new Item
        {
            ProjectId = request.ProjectId,
            ItemType = Domain.Enums.ItemSubtype.Defect,
            ParentFeatureId = request.ParentFeatureId,
            Title = request.Title,
            Description = request.Description,
            AcceptanceCriteria = request.AcceptanceCriteria,
            Plan = request.Plan,
            SecurityImpact = request.SecurityImpact,
            PerformanceImpact = request.PerformanceImpact,
            TestPlan = request.TestPlan,
            DeploymentPlan = request.DeploymentPlan,
            OpenQuestions = request.OpenQuestions,
            Severity = request.Severity,
            Status = request.InitialStatus ?? FeatureStatus.Planning,
            DependsOnId = request.DependsOnId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}

public class UpdateDefectHandler : IUpdateDefectHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateDefectHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(UpdateDefectCommand request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

       if (request.Title is not null) item.Title = request.Title;
        if (request.Description is not null) item.Description = request.Description;
        if (request.AcceptanceCriteria is not null) item.AcceptanceCriteria = request.AcceptanceCriteria;
        if (request.Plan is not null) item.Plan = request.Plan;
        if (request.SecurityImpact is not null) item.SecurityImpact = request.SecurityImpact;
        if (request.PerformanceImpact is not null) item.PerformanceImpact = request.PerformanceImpact;
        if (request.TestPlan is not null) item.TestPlan = request.TestPlan;
        if (request.DeploymentPlan is not null) item.DeploymentPlan = request.DeploymentPlan;
        if (request.OpenQuestions is not null) item.OpenQuestions = request.OpenQuestions;
        if (request.Severity is not null) item.Severity = request.Severity;
        if (request.RootCause is not null) item.RootCause = request.RootCause;
        if (request.DependsOnId is not null) item.DependsOnId = request.DependsOnId;

        item.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class TransitionDefectStatusHandler : ITransitionDefectStatusHandler
{
    private readonly DevStackDbContext _dbContext;
    private readonly ItemStatusTransitionService _transitionService;

    public TransitionDefectStatusHandler(DevStackDbContext dbContext, ItemStatusTransitionService transitionService)
    {
        _dbContext = dbContext;
        _transitionService = transitionService;
    }

    public async global::System.Threading.Tasks.Task Handle(TransitionDefectStatusCommand request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

        var result = _transitionService.Transition(item, request.TargetStatus, request.Actor);

        if (!result.IsSuccess)
            throw new InvalidOperationException($"Transition failed: {string.Join(", ", result.Errors)}");

        _dbContext.Items.Update(item);
        
        foreach (var @event in _transitionService.DomainEvents)
        {
            _dbContext.AuditEvents.Add(new AuditEvent
            {
                EntityType = "Item",
                EntityId = item.Id,
                EventType = "StatusChanged",
                OldValue = item.Status.ToString(),
                NewValue = request.TargetStatus.ToString(),
                Actor = request.Actor,
                OccurredAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteDefectHandler : IDeleteDefectHandler
{
    private readonly DevStackDbContext _dbContext;

    public DeleteDefectHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(DeleteDefectCommand request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

        _dbContext.Items.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
