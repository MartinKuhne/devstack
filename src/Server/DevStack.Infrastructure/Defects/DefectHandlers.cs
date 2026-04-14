using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace DevStack.Infrastructure.Defects;

public record CreateDefectCommand(
    Guid ProjectId,
    Guid? ParentFeatureId,
    Severity? Severity,
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
    string? OpenQuestions);

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
        var defect = new Defect
        {
            ProjectId = request.ProjectId,
            ParentFeatureId = request.ParentFeatureId,
            Severity = request.Severity,
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

        _dbContext.Defects.Add(defect);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return defect.Id;
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
        var defect = await _dbContext.Defects.FindAsync([request.Id], cancellationToken);
        if (defect == null)
            throw new InvalidOperationException($"NOT_FOUND: Defect with ID {request.Id} not found.");

        if (!string.IsNullOrEmpty(request.Title)) defect.Title = request.Title;
        if (request.Description is not null) defect.Description = request.Description;
        if (request.AcceptanceCriteria is not null) defect.AcceptanceCriteria = request.AcceptanceCriteria;
        if (request.Plan is not null) defect.Plan = request.Plan;
        if (request.SecurityImpact is not null) defect.SecurityImpact = request.SecurityImpact;
        if (request.PerformanceImpact is not null) defect.PerformanceImpact = request.PerformanceImpact;
        if (request.TestPlan is not null) defect.TestPlan = request.TestPlan;
        if (request.DeploymentPlan is not null) defect.DeploymentPlan = request.DeploymentPlan;
        if (request.OpenQuestions is not null) defect.OpenQuestions = request.OpenQuestions;

        defect.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class TransitionDefectStatusHandler : ITransitionDefectStatusHandler
{
    private readonly DevStackDbContext _dbContext;
    private readonly FeatureStatusTransitionService _transitionService;

    public TransitionDefectStatusHandler(DevStackDbContext dbContext, FeatureStatusTransitionService transitionService)
    {
        _dbContext = dbContext;
        _transitionService = transitionService;
    }

    public async global::System.Threading.Tasks.Task Handle(TransitionDefectStatusCommand request, CancellationToken cancellationToken)
    {
        var defect = await _dbContext.Defects.FindAsync([request.Id], cancellationToken);
        if (defect == null)
            throw new InvalidOperationException($"Defect with ID {request.Id} not found.");

        var result = _transitionService.Transition(defect, request.TargetStatus, request.Actor);

        if (!result.IsSuccess)
            throw new InvalidOperationException($"FEATURE_VALIDATION_ERROR: Transition failed: {string.Join(", ", result.Errors)}");

        _dbContext.Defects.Update(defect);
        
        foreach (var @event in _transitionService.DomainEvents)
        {
            _dbContext.AuditEvents.Add(new AuditEvent
            {
                EntityType = "Defect",
                EntityId = defect.Id,
                EventType = "StatusChanged",
                OldValue = defect.Status.ToString(),
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
        var defect = await _dbContext.Defects.FindAsync([request.Id], cancellationToken);
        if (defect == null)
            throw new InvalidOperationException($"NOT_FOUND: Defect with ID {request.Id} not found.");

        _dbContext.Defects.Remove(defect);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}