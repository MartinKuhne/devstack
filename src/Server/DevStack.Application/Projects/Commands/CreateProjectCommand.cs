namespace DevStack.Application.Projects.Commands;

public record CreateProjectCommand(
    string Name,
    string? Description,
    string? Architecture,
    string? Memory,
    string? GithubUrl);
