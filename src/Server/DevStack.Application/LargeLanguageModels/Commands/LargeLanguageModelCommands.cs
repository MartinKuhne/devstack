namespace DevStack.Application.LargeLanguageModels.Commands;

public record CreateLargeLanguageModelCommand(
    string Url,
    string Model,
    string? ModelAlias,
    string ApiKey,
    int Cost,
    int MaxComplexity,
    int MaxConcurrency);

public record UpdateLargeLanguageModelCommand(
    Guid Id,
    string? Url,
    string? Model,
    string? ModelAlias,
    string? ApiKey,
    int? Cost,
    int? MaxComplexity,
    int? MaxConcurrency);

public record DeleteLargeLanguageModelCommand(Guid Id);
