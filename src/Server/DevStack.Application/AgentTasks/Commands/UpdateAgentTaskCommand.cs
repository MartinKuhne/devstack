namespace DevStack.Application.AgentTasks.Commands;

public record UpdateAgentTaskCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? Result,
    string? Errors,
    string? CommitHash,
    int? ComplexityRating,
    Guid? DependsOnAgentTaskId,
    int? PromptTokens,
    int? CompletionTokens,
    int? ExecutionDurationInSeconds,
    string? Agent);
