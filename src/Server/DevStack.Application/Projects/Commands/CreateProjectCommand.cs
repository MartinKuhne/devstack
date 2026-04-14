namespace DevStack.Application.Projects.Commands;

public record CreateProjectCommand(
    string Name,
    string? Description,
    string? Architecture,
    string? Memory,
    string? GithubUrl);

public interface ICreateProjectHandler
{
    Task<Guid> Handle(CreateProjectCommand command, CancellationToken cancellationToken = default);
}
