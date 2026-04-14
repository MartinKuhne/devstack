using DevStack.Domain.Entities;
using DevStack.Infrastructure.Persistence;
using Task = System.Threading.Tasks.Task;

namespace DevStack.Infrastructure.Projects;

public record UpdateProjectCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? Architecture,
    string? Memory,
    string? GithubUrl,
    string? GithubToken_Encrypted);

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
        if (request.Architecture is not null) project.Architecture = request.Architecture;
        if (request.Memory is not null) project.Memory = request.Memory;
        
        if (!string.IsNullOrEmpty(request.GithubUrl))
        {
            if (!Uri.TryCreate(request.GithubUrl, UriKind.Absolute, out var uri))
                throw new ArgumentException("GitHub URL is not a valid URI", nameof(request.GithubUrl));
            project.GithubUrl = uri;
        }
        else
        {
            project.GithubUrl = null;
        }

        if (request.GithubToken_Encrypted is not null)
        {
            project.GithubToken_Encrypted = request.GithubToken_Encrypted;
        }

        project.UpdatedAt = DateTime.UtcNow;

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
