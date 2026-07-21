namespace DevStack.Mcp.Dto;

public record ProjectDto(string Id, string Name, string? Description, string? Repository);

public record CreateProjectResponse(string Id, string Name, string? Description, string? Repository);

public record ProjectListResponse(IReadOnlyList<ProjectDto> Projects);
