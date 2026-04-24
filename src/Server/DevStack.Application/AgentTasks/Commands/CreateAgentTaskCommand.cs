namespace DevStack.Application.AgentTasks.Commands;

public record CreateAgentTaskCommand(
    Guid ProjectId,
    Guid DeliverableId,
    string Title,
    string Description,
    int ComplexityRating,
    Guid? DependsOnAgentTaskId);
