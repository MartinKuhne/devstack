namespace DevStack.Application.Projects.Commands;

public record CreateProjectCommand(
    string Name,
    string? Description,
    string? Repository);
