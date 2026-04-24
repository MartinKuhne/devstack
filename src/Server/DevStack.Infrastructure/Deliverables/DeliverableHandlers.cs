using DevStack.Application;
using DevStack.Application.Deliverables;
using DevStack.Application.Deliverables.Commands;
using DevStack.Application.Deliverables.Queries;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Persistence;

namespace DevStack.Infrastructure.Deliverables;

public class CreateDeliverableHandler : ICommandHandler<Guid, CreateDeliverableCommand>
{
    private readonly DevStackDbContext _dbContext;

    public CreateDeliverableHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateDeliverableCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ArgumentException("Title is required", nameof(command.Title));

        if (command.Title.Length > 200)
            throw new ArgumentException("Title must be 200 characters or less", nameof(command.Title));

        var project = await _dbContext.Projects.FindAsync([command.ProjectId], cancellationToken);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {command.ProjectId} not found.");

        var deliverable = new Deliverable
        {
            ProjectId = command.ProjectId,
            Type = command.Type,
            Title = command.Title,
            Description = command.Description,
            AcceptanceCriteria = command.AcceptanceCriteria,
            ExecutionPlan = command.ExecutionPlan,
            SecurityImpact = command.SecurityImpact,
            PerformanceImpact = command.PerformanceImpact,
            TestPlan = command.TestPlan,
            DeploymentPlan = command.DeploymentPlan,
            Status = DeliverableStatus.Draft
        };

        _dbContext.Deliverables.Add(deliverable);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return deliverable.Id;
    }
}

public class UpdateDeliverableHandler : ICommandHandler<UpdateDeliverableCommand>
{
    private readonly DevStackDbContext _dbContext;

    public UpdateDeliverableHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateDeliverableCommand command, CancellationToken cancellationToken)
    {
        var deliverable = await _dbContext.Deliverables.FindAsync([command.Id], cancellationToken);
        if (deliverable == null)
            throw new InvalidOperationException($"Deliverable with ID {command.Id} not found.");

        if (!string.IsNullOrWhiteSpace(command.Title))
        {
            if (command.Title.Length > 200)
                throw new ArgumentException("Title must be 200 characters or less", nameof(command.Title));

            deliverable.Title = command.Title;
        }

        if (command.Description is not null) deliverable.Description = command.Description;
        if (command.AcceptanceCriteria is not null) deliverable.AcceptanceCriteria = command.AcceptanceCriteria;
        if (command.ExecutionPlan is not null) deliverable.ExecutionPlan = command.ExecutionPlan;
        if (command.AgentFeedback is not null) deliverable.AgentFeedback = command.AgentFeedback;
        if (command.SecurityImpact is not null) deliverable.SecurityImpact = command.SecurityImpact;
        if (command.PerformanceImpact is not null) deliverable.PerformanceImpact = command.PerformanceImpact;
        if (command.TestPlan is not null) deliverable.TestPlan = command.TestPlan;
        if (command.DeploymentPlan is not null) deliverable.DeploymentPlan = command.DeploymentPlan;
        if (command.Blocking is not null) deliverable.Blocking = command.Blocking;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class UpdateDeliverableStatusHandler : ICommandHandler<UpdateDeliverableStatusCommand>
{
    private readonly DevStackDbContext _dbContext;
    private readonly DeliverableStatusTransitionService _transitionService;

    public UpdateDeliverableStatusHandler(DevStackDbContext dbContext, DeliverableStatusTransitionService transitionService)
    {
        _dbContext = dbContext;
        _transitionService = transitionService;
    }

    public async Task Handle(UpdateDeliverableStatusCommand command, CancellationToken cancellationToken)
    {
        var deliverable = await _dbContext.Deliverables.FindAsync([command.Id], cancellationToken);
        if (deliverable == null)
            throw new InvalidOperationException($"Deliverable with ID {command.Id} not found.");

        var result = _transitionService.Transition(deliverable, command.TargetStatus, command.ChangedBy);

        if (!result.IsSuccess)
            throw new InvalidOperationException(string.Join("; ", result.Errors));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteDeliverableHandler : ICommandHandler<DeleteDeliverableCommand>
{
    private readonly DevStackDbContext _dbContext;

    public DeleteDeliverableHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteDeliverableCommand command, CancellationToken cancellationToken)
    {
        var deliverable = await _dbContext.Deliverables.FindAsync([command.Id], cancellationToken);
        if (deliverable == null)
            throw new InvalidOperationException($"Deliverable with ID {command.Id} not found.");

        _dbContext.Deliverables.Remove(deliverable);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class GetDeliverableByIdHandler : IGetDeliverableByIdHandler
{
    private readonly DevStackDbContext _dbContext;

    public GetDeliverableByIdHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Deliverable?> Handle(GetDeliverableByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Deliverables.FindAsync([query.Id], cancellationToken);
    }
}
