using DevStack.Domain.Entities;
using DevStack.Persistence;

namespace DevStack.Infrastructure.Projects;

public record CreateProjectCommand(
    string Name,
    string? Description,
    string? Repository);

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

        var project = new Project
        {
            Name = request.Name!,
            Description = request.Description,
            Repository = request.Repository ?? string.Empty
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
