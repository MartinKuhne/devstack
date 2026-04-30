using DevStack.Domain.Exceptions;

using HotChocolate;

namespace DevStack.Api.GraphQL;

/// <summary>
/// Pure function for classifying GraphQL errors based on their type and properties.
/// </summary>
public static class ErrorClassifier
{
    /// <summary>
    /// Determines the log level for a GraphQL error based on whether it has an exception.
    /// </summary>
    /// <param name="error">The GraphQL error to classify.</param>
    /// <returns>True if the error has an exception (should be logged as Error), false otherwise (Warning).</returns>
    public static bool IsExceptionError(IError error)
    {
        return error.Exception != null;
    }

    /// <summary>
    /// Classifies an error into a human-readable category for logging purposes.
    /// </summary>
    /// <param name="error">The GraphQL error to classify.</param>
    /// <returns>A string representing the error category.</returns>
    public static string Classify(IError error)
    {
        if (error.Exception != null)
        {
            return error.Exception switch
            {
                NotFoundException => "NotFoundError",
                ValidationException => "ValidationError",
                _ => "Exception"
            };
        }

        return error.Code switch
        {
            "validation" => "ValidationError",
            "not_found" => "NotFoundError",
            "unauthorized" => "UnauthorizedError",
            _ => "GraphQLError"
        };
    }
}

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
        var classification = ErrorClassifier.Classify(error);
        var isException = ErrorClassifier.IsExceptionError(error);

        if (isException)
        {
            _logger.LogError(
                error.Exception,
                "GraphQL error - Type: {Type}, Message: {Message}, Code: {Code}, Path: {Path}",
                classification,
                error.Message,
                error.Code,
                error.Path);
        }
        else
        {
            _logger.LogWarning(
                "GraphQL error - Type: {Type}, Message: {Message}, Code: {Code}, Path: {Path}",
                classification,
                error.Message,
                error.Code,
                error.Path);
        }

        return error;
    }
}
