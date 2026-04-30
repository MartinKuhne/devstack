namespace DevStack.Domain.Validation;

public record ValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors);
