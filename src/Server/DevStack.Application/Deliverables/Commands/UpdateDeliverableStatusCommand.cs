namespace DevStack.Application.Deliverables.Commands;

public record UpdateDeliverableStatusCommand(
    Guid Id,
    Domain.Enums.DeliverableStatus TargetStatus,
    string ChangedBy);
