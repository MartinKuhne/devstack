using DevStack.Application.Projects.Commands;

namespace DevStack.Infrastructure.Projects;

public class CreateProjectHandler : ICommandHandler<Guid, CreateProjectCommand>
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
            Description = request.Description ?? string.Empty,
            Repository = request.Repository ?? string.Empty
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
