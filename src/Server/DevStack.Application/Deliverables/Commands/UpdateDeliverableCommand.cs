namespace DevStack.Application.Deliverables.Commands;

public record UpdateDeliverableCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? AcceptanceCriteria,
    string? ExecutionPlan,
    string? AgentFeedback,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? Blocking);
