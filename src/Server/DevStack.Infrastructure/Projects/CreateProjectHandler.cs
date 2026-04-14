using DevStack.Domain.Entities;
using DevStack.Infrastructure.Persistence;

namespace DevStack.Infrastructure.Projects;

public record CreateProjectCommand(
    string Name,
    string? Description,
    string? Architecture,
    string? Memory,
    string? GithubUrl);

public interface ICreateProjectHandler : DevStack.Application.ICommandHandler<Guid, CreateProjectCommand>
{
}

public class CreateProjectHandler : ICreateProjectHandler
{
    private readonly DevStackDbContext _dbContext;

    public CreateProjectHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required", nameof(request.Name));

        if (request.Name.Length > 200)
            throw new ArgumentException("Name must be 200 characters or less", nameof(request.Name));

        Uri? githubUri = null;
        if (!string.IsNullOrEmpty(request.GithubUrl))
        {
            if (!Uri.TryCreate(request.GithubUrl, UriKind.Absolute, out var uri))
                throw new ArgumentException("GitHub URL is not a valid URI", nameof(request.GithubUrl));
            githubUri = uri;
        }

        var project = new Project
        {
            Name = request.Name!,
            Description = request.Description,
            Architecture = request.Architecture,
            Memory = request.Memory!,
            GithubUrl = githubUri,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}