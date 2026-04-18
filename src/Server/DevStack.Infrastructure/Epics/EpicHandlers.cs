using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;

namespace DevStack.Infrastructure.Epics;

public record CreateEpicCommand(Guid ProjectId, string Title, string? Description, FeatureStatus Status);

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

    public async global::System.Threading.Tasks.Task<Guid> Handle(CreateEpicCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required", nameof(request.Title));

        if (request.Title.Length > 200)
            throw new ArgumentException("Title must be 200 characters or less", nameof(request.Title));

       var item = new Item
        {
            ProjectId = request.ProjectId,
            Subtype = ItemSubtype.Epic,
            Title = request.Title!,
            Description = request.Description,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}

public class UpdateEpicHandler : IUpdateEpicHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateEpicHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async global::System.Threading.Tasks.Task Handle(UpdateEpicCommand request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

       if (!string.IsNullOrEmpty(request.Title)) item.Title = request.Title;
        if (request.Description is not null) item.Description = request.Description;

        item.UpdatedAt = DateTime.UtcNow;

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

    public async global::System.Threading.Tasks.Task Handle(DeleteEpicCommand request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.FindAsync([request.Id], cancellationToken);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {request.Id} not found.");

        _dbContext.Items.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
