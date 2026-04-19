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
    FeatureStatus? InitialStatus,
    Guid? DependsOnId);

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
    string? OpenQuestions,
    Guid? DependsOnId);

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
        var item = new Item
        {
            ProjectId = request.ProjectId,
            ItemType = Domain.Enums.ItemSubtype.Feature,
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
            DependsOnId = request.DependsOnId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return item.Id;
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
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

        if (!string.IsNullOrEmpty(request.Title)) item.Title = request.Title;
        if (request.Description is not null) item.Description = request.Description;
        if (request.AcceptanceCriteria is not null) item.AcceptanceCriteria = request.AcceptanceCriteria;
        if (request.Plan is not null) item.Plan = request.Plan;
        if (request.SecurityImpact is not null) item.SecurityImpact = request.SecurityImpact;
        if (request.PerformanceImpact is not null) item.PerformanceImpact = request.PerformanceImpact;
        if (request.TestPlan is not null) item.TestPlan = request.TestPlan;
        if (request.DeploymentPlan is not null) item.DeploymentPlan = request.DeploymentPlan;
        if (request.OpenQuestions is not null) item.OpenQuestions = request.OpenQuestions;
        if (request.DependsOnId is not null) item.DependsOnId = request.DependsOnId;

        item.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class TransitionFeatureStatusHandler : ITransitionFeatureStatusHandler
{
    private readonly DevStackDbContext _dbContext;
    private readonly ItemStatusTransitionService _transitionService;

    public TransitionFeatureStatusHandler(DevStackDbContext dbContext, ItemStatusTransitionService transitionService)
    {
        _dbContext = dbContext;
        _transitionService = transitionService;
    }

    public async global::System.Threading.Tasks.Task Handle(TransitionFeatureStatusCommand request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

        var result = _transitionService.Transition(item, request.TargetStatus, request.Actor);

        if (!result.IsSuccess)
            throw new InvalidOperationException($"Transition failed: {string.Join(", ", result.Errors)}");

        _dbContext.Items.Update(item);
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
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

        _dbContext.Items.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}