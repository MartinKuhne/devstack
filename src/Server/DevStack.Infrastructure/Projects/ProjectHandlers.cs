using DevStack.Domain.Entities;
using DevStack.Persistence;
using Task = System.Threading.Tasks.Task;

namespace DevStack.Infrastructure.Projects;

public record UpdateProjectCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? Repository);

public record DeleteProjectCommand(Guid Id);

public interface IUpdateProjectHandler : DevStack.Application.ICommandHandler<UpdateProjectCommand>
{
}

public interface IDeleteProjectHandler : DevStack.Application.ICommandHandler<DeleteProjectCommand>
{
}

public interface IGetProjectByIdHandler
{
    Task<Project?> Handle(Guid id, CancellationToken cancellationToken = default);
}

public class UpdateProjectHandler : IUpdateProjectHandler
{
    private readonly DevStackDbContext _dbContext;

    public UpdateProjectHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FindAsync([request.Id], cancellationToken);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.Id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            if (request.Name.Length > 200)
                throw new ArgumentException("Name must be 200 characters or less", nameof(request.Name));
            
            project.Name = request.Name;
        }

        if (request.Description is not null) project.Description = request.Description;
        if (request.Repository is not null) project.Repository = request.Repository;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteProjectHandler : IDeleteProjectHandler
{
    private readonly DevStackDbContext _dbContext;

    public DeleteProjectHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FindAsync([request.Id], cancellationToken);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.Id} not found.");

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class GetProjectByIdHandler : IGetProjectByIdHandler
{
    private readonly DevStackDbContext _dbContext;

    public GetProjectByIdHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Project?> Handle(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Projects.FindAsync([id], cancellationToken);
    }
}
