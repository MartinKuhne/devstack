using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace DevStack.Infrastructure.Features;

public record CreateFeatureCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    string? AcceptanceCriteria,
    string? Plan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? OpenQuestions,
    FeatureStatus? InitialStatus);

public record UpdateFeatureCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? AcceptanceCriteria,
    string? Plan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? OpenQuestions);

public record TransitionFeatureStatusCommand(
    Guid Id,
    FeatureStatus TargetStatus,
    string Actor);

public record DeleteFeatureCommand(Guid Id);

public interface ICreateFeatureHandler : DevStack.Application.ICommandHandler<Guid, CreateFeatureCommand>
{
}

public interface IUpdateFeatureHandler : DevStack.Application.ICommandHandler<UpdateFeatureCommand>
{
}

public interface ITransitionFeatureStatusHandler : DevStack.Application.ICommandHandler<TransitionFeatureStatusCommand>
{
}

public interface IDeleteFeatureHandler : DevStack.Application.ICommandHandler<DeleteFeatureCommand>
{
}

public class CreateFeatureHandler : ICreateFeatureHandler
{
    private readonly DevStackDbContext _dbContext;

    public CreateFeatureHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task<Guid> Handle(CreateFeatureCommand request, CancellationToken cancellationToken)
    {
        var feature = new Feature
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            AcceptanceCriteria = request.AcceptanceCriteria,
            Plan = request.Plan,
            SecurityImpact = request.SecurityImpact,
            PerformanceImpact = request.PerformanceImpact,
            TestPlan = request.TestPlan,
            DeploymentPlan = request.DeploymentPlan,
            OpenQuestions = request.OpenQuestions,
            Status = request.InitialStatus ?? FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Features.Add(feature);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return feature.Id;
    }
}

public class UpdateFeatureHandler : IUpdateFeatureHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateFeatureHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(UpdateFeatureCommand request, CancellationToken cancellationToken)
    {
        var feature = await _dbContext.Features.FindAsync([request.Id], cancellationToken);
        if (feature == null)
            throw new InvalidOperationException($"Feature with ID {request.Id} not found.");

        if (!string.IsNullOrEmpty(request.Title)) feature.Title = request.Title;
        if (request.Description is not null) feature.Description = request.Description;
        if (request.AcceptanceCriteria is not null) feature.AcceptanceCriteria = request.AcceptanceCriteria;
        if (request.Plan is not null) feature.Plan = request.Plan;
        if (request.SecurityImpact is not null) feature.SecurityImpact = request.SecurityImpact;
        if (request.PerformanceImpact is not null) feature.PerformanceImpact = request.PerformanceImpact;
        if (request.TestPlan is not null) feature.TestPlan = request.TestPlan;
        if (request.DeploymentPlan is not null) feature.DeploymentPlan = request.DeploymentPlan;
        if (request.OpenQuestions is not null) feature.OpenQuestions = request.OpenQuestions;

        feature.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class TransitionFeatureStatusHandler : ITransitionFeatureStatusHandler
{
    private readonly DevStackDbContext _dbContext;
    private readonly FeatureStatusTransitionService _transitionService;

    public TransitionFeatureStatusHandler(DevStackDbContext dbContext, FeatureStatusTransitionService transitionService)
    {
        _dbContext = dbContext;
        _transitionService = transitionService;
    }

    public async global::System.Threading.Tasks.Task Handle(TransitionFeatureStatusCommand request, CancellationToken cancellationToken)
    {
        var feature = await _dbContext.Features.FindAsync([request.Id], cancellationToken);
        if (feature == null)
            throw new InvalidOperationException($"Feature with ID {request.Id} not found.");

        var result = _transitionService.Transition(feature, request.TargetStatus, request.Actor);

        if (!result.IsSuccess)
            throw new InvalidOperationException($"Transition failed: {string.Join(", ", result.Errors)}");

        _dbContext.Features.Update(feature);
        
        foreach (var @event in _transitionService.DomainEvents)
        {
            _dbContext.AuditEvents.Add(new AuditEvent
            {
                EntityType = "Feature",
                EntityId = feature.Id,
                EventType = "StatusChanged",
                OldValue = feature.Status.ToString(),
                NewValue = request.TargetStatus.ToString(),
                Actor = request.Actor,
                OccurredAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteFeatureHandler : IDeleteFeatureHandler
{
    private readonly DevStackDbContext _dbContext;

    public DeleteFeatureHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(DeleteFeatureCommand request, CancellationToken cancellationToken)
    {
        var feature = await _dbContext.Features.FindAsync([request.Id], cancellationToken);
        if (feature == null)
            throw new InvalidOperationException($"Feature with ID {request.Id} not found.");

        _dbContext.Features.Remove(feature);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}