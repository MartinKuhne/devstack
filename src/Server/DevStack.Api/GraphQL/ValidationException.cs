using FluentValidation.Results;

namespace DevStack.Api.GraphQL;

public class ValidationException : Exception
{
    public ValidationResult ValidationResult { get; }

    public ValidationException(ValidationResult validationResult)
        : base(GetMessage(validationResult))
    {
        ValidationResult = validationResult;
    }

    private static string GetMessage(ValidationResult validationResult)
    {
        var messages = validationResult.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
            .ToList();

        return string.Join("; ", messages);
    }
}
