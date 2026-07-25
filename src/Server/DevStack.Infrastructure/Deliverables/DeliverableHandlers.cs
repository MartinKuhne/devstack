using DevStack.Application.Deliverables;
using DevStack.Application.Deliverables.Commands;
using DevStack.Application.Deliverables.Queries;

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

        var deliverable = new Deliverable(
            projectId: command.ProjectId,
            type: command.Type,
            title: command.Title,
            status: command.InitialStatus,
            description: command.Description,
            design: command.Design,
            acceptanceCriteria: command.AcceptanceCriteria,
            executionPlan: command.ExecutionPlan,
            securityImpact: command.SecurityImpact,
            performanceImpact: command.PerformanceImpact,
            testPlan: command.TestPlan,
            deploymentPlan: command.DeploymentPlan);

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

        deliverable.UpdateMetadata(
            title: command.Title,
            description: command.Description,
            acceptanceCriteria: command.AcceptanceCriteria,
            executionPlan: command.ExecutionPlan,
            agentFeedback: command.AgentFeedback,
            securityImpact: command.SecurityImpact,
            performanceImpact: command.PerformanceImpact,
            testPlan: command.TestPlan,
            deploymentPlan: command.DeploymentPlan,
            blocking: command.Blocking,
            design: command.Design);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class UpdateDeliverableStatusHandler : ICommandHandler<UpdateDeliverableStatusCommand>
{
    private readonly DevStackDbContext _dbContext;

    public UpdateDeliverableStatusHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateDeliverableStatusCommand command, CancellationToken cancellationToken)
    {
        var deliverable = await _dbContext.Deliverables.FindAsync([command.Id], cancellationToken);
        if (deliverable == null)
            throw new InvalidOperationException($"Deliverable with ID {command.Id} not found.");

        deliverable.TransitionStatus(command.TargetStatus);
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

public class GetDeliverableByIdHandler : ICommandHandler<Deliverable?, GetDeliverableByIdQuery>
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
