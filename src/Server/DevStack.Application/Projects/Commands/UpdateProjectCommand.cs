namespace DevStack.Application.Projects.Commands;

public record UpdateProjectCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? Repository);
