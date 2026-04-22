using FluentValidation.Results;
using HotChocolate;
using Microsoft.Extensions.Logging;

namespace DevStack.Api.GraphQL;

/// <summary>
/// Error filter for HotChocolate GraphQL to log errors and map FluentValidation errors to field-level errors.
/// </summary>
public class GraphQLErrorFilter : IErrorFilter
{
    private readonly ILogger<GraphQLErrorFilter> _logger;

    public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger)
    {
        _logger = logger;
    }

    public IError OnError(IError error)
    {
        if (error.Exception is ValidationException validationException)
        {
            var fieldErrors = MapToFieldErrors(validationException.ValidationResult);
            var fieldErrorMessages = fieldErrors.Select(e => $"{e.Field}: {e.Message}").ToList();

            _logger.LogWarning(
                "FluentValidation error - Fields: {Fields}",
                fieldErrorMessages);

            return error.WithMessage(
                $"Validation failed: {string.Join(", ", fieldErrors.Select(e => $"{e.Field}: {e.Message}"))}");
        }

        if (error.Exception != null)
        {
            _logger.LogError(
                error.Exception,
                "GraphQL error - Message: {Message}, Code: {Code}, Path: {Path}",
                error.Message,
                error.Code,
                error.Path);
        }
        else
        {
            _logger.LogWarning(
                "GraphQL error - Message: {Message}, Code: {Code}, Path: {Path}",
                error.Message,
                error.Code,
                error.Path);
        }

        return error;
    }

    private static List<DevStack.Api.GraphQL.Types.FieldError> MapToFieldErrors(ValidationResult validationResult)
    {
        var errors = new List<DevStack.Api.GraphQL.Types.FieldError>();

        if (validationResult.IsValid)
            return errors;

        foreach (var failure in validationResult.Errors)
        {
            var fieldName = !string.IsNullOrWhiteSpace(failure.PropertyName)
                ? failure.PropertyName!
                : "ValidationError";

            errors.Add(new DevStack.Api.GraphQL.Types.FieldError(fieldName, failure.ErrorMessage!));
        }

        return errors;
    }
}
