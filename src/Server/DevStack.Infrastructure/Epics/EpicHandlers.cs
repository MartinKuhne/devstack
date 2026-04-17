using DevStack.Domain.Entities;
using DevStack.Infrastructure.Persistence;

namespace DevStack.Infrastructure.Epics;

public record CreateEpicCommand(Guid ProjectId, string Title, string? Description);

public record UpdateEpicCommand(Guid Id, string? Title, string? Description);

public record DeleteEpicCommand(Guid Id);

public interface ICreateEpicHandler : DevStack.Application.ICommandHandler<Guid, CreateEpicCommand>
{
}

public interface IUpdateEpicHandler : DevStack.Application.ICommandHandler<UpdateEpicCommand>
{
}

public interface IDeleteEpicHandler : DevStack.Application.ICommandHandler<DeleteEpicCommand>
{
}

public class CreateEpicHandler : ICreateEpicHandler
{
    private readonly DevStackDbContext _dbContext;

    public CreateEpicHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateEpicCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required", nameof(request.Title));

        if (request.Title.Length > 200)
            throw new ArgumentException("Title must be 200 characters or less", nameof(request.Title));

        var epic = new Epic
        {
            ProjectId = request.ProjectId,
            Title = request.Title!,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Epics.Add(epic);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return epic.Id;
    }
}

public class UpdateEpicHandler : IUpdateEpicHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateEpicHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateEpicCommand request, CancellationToken cancellationToken)
    {
        var epic = await _dbContext.Epics.FindAsync([request.Id], cancellationToken);
        if (epic == null)
            throw new InvalidOperationException($"Epic with ID {request.Id} not found.");

        if (!string.IsNullOrEmpty(request.Title)) epic.Title = request.Title;
        if (request.Description is not null) epic.Description = request.Description;

        epic.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteEpicHandler : IDeleteEpicHandler
{
    private readonly DevStackDbContext _dbContext;

    public DeleteEpicHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteEpicCommand request, CancellationToken cancellationToken)
    {
        var epic = await _dbContext.Epics.FindAsync([request.Id], cancellationToken);
        if (epic == null)
            throw new InvalidOperationException($"Epic with ID {request.Id} not found.");

        _dbContext.Epics.Remove(epic);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
