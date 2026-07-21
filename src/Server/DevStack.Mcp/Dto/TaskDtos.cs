namespace DevStack.Mcp.Dto;

public record AgentTaskDto(
    string Id,
    string ProjectId,
    string Title,
    string Status,
    string? Description,
    string? Result,
    string? Errors,
    string? CommitHash,
    string? Agent);

public record CreateAgentTaskResponse(string Id, string Status);

public record UpdateAgentTaskResponse(string Id, bool Updated);

public record TransitionAgentTaskStatusResponse(string Id, string Status, string Actor);
