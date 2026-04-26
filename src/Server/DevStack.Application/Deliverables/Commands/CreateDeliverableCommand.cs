namespace DevStack.Application.Deliverables.Commands;

public record CreateDeliverableCommand(
    Guid ProjectId,
    Domain.Enums.DeliverableType Type,
    string Title,
    string? Description,
    string? AcceptanceCriteria,
    string? ExecutionPlan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    Domain.Enums.DeliverableStatus InitialStatus);
