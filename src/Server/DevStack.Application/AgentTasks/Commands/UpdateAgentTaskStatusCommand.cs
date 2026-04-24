namespace DevStack.Application.AgentTasks.Commands;

public record UpdateAgentTaskStatusCommand(
    Guid Id,
    Domain.Enums.AgentTaskStatus Status,
    string Actor);
