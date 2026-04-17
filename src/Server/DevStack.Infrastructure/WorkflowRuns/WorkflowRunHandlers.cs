using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace DevStack.Infrastructure.WorkflowRuns;

public record CreateWorkflowRunCommand(
    Guid ProjectId,
    Guid? ItemId,
    Guid? TaskId,
    WorkflowType WorkflowType,
    string InputPayload);

public record UpdateWorkflowRunCommand(
    Guid Id,
    WorkflowRunStatus Status,
    string? OutputPayload);

public record CancelWorkflowRunCommand(Guid Id);

public interface ICreateWorkflowRunHandler : DevStack.Application.ICommandHandler<Guid, CreateWorkflowRunCommand>
{
}

public interface IUpdateWorkflowRunHandler : DevStack.Application.ICommandHandler<UpdateWorkflowRunCommand>
{
}

public interface ICancelWorkflowRunHandler : DevStack.Application.ICommandHandler<CancelWorkflowRunCommand>
{
}

public class CreateWorkflowRunHandler : ICreateWorkflowRunHandler
{
    private readonly DevStackDbContext _dbContext;

    public CreateWorkflowRunHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task<Guid> Handle(CreateWorkflowRunCommand request, CancellationToken cancellationToken)
    {
        var run = new WorkflowRun
        {
            ProjectId = request.ProjectId,
            ItemId = request.ItemId,
            TaskId = request.TaskId,
            WorkflowType = request.WorkflowType,
            Status = WorkflowRunStatus.Queued,
            InputPayload = request.InputPayload,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.WorkflowRuns.Add(run);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return run.Id;
    }
}

public class UpdateWorkflowRunHandler : IUpdateWorkflowRunHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateWorkflowRunHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(UpdateWorkflowRunCommand request, CancellationToken cancellationToken)
    {
        var run = await _dbContext.WorkflowRuns.FindAsync([request.Id], cancellationToken);
        if (run == null)
            throw new InvalidOperationException($"WorkflowRun with ID {request.Id} not found.");

        run.Status = request.Status;
        if (request.OutputPayload is not null) run.OutputPayload = request.OutputPayload;

        if (request.Status == WorkflowRunStatus.Succeeded || request.Status == WorkflowRunStatus.Failed)
            run.CompletedAt = DateTime.UtcNow;

        _dbContext.WorkflowRuns.Update(run);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class CancelWorkflowRunHandler : ICancelWorkflowRunHandler
{
    private readonly DevStackDbContext _dbContext;

    public CancelWorkflowRunHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(CancelWorkflowRunCommand request, CancellationToken cancellationToken)
    {
        var run = await _dbContext.WorkflowRuns.FindAsync([request.Id], cancellationToken);
        if (run == null)
            throw new InvalidOperationException($"WorkflowRun with ID {request.Id} not found.");

        run.Status = WorkflowRunStatus.Cancelled;
        run.CompletedAt = DateTime.UtcNow;

        _dbContext.WorkflowRuns.Update(run);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}