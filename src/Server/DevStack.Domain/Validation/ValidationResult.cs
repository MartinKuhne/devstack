namespace DevStack.Domain.Validation;

public record ValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors)
{
    public static ValidationResult Valid { get; } = new(true, []);
}
