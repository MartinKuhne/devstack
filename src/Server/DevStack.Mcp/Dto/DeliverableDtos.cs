namespace DevStack.Mcp.Dto;

public record DeliverableDto(
    string Id,
    string ProjectId,
    string Title,
    string? Description,
    string? Design,
    string? AcceptanceCriteria,
    string? ExecutionPlan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? AgentFeedback,
    string? Blocking);

public record CreateDeliverableResponse(string Id, string ProjectId, string Type, string Status);

public record UpdateDeliverableResponse(string Id, bool Updated);

public record GetDeliverableResponse(
    string Id,
    string ProjectId,
    string Title,
    string? Description,
    string? Design,
    string? AcceptanceCriteria,
    string? ExecutionPlan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? AgentFeedback,
    string? Blocking);

public record TransitionDeliverableStatusResponse(string Id, string Status, string Actor);
