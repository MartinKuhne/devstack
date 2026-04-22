using FluentValidation.Results;

namespace DevStack.Api.GraphQL.Types;

public static class FieldErrorMapper
{
    public static List<FieldError> Map(ValidationResult validationResult)
    {
        var errors = new List<FieldError>();

        if (validationResult.IsValid)
            return errors;

        foreach (var failure in validationResult.Errors)
        {
            var fieldName = !string.IsNullOrWhiteSpace(failure.PropertyName)
                ? failure.PropertyName!
                : "ValidationError";

            errors.Add(new FieldError(fieldName, failure.ErrorMessage!));
        }

        return errors;
    }
}
