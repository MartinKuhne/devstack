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
}
