using System.Net;

using ModelContextProtocol;

namespace DevStack.Mcp.Logging;

public class McpExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<McpExceptionHandlingMiddleware> _logger;

    public McpExceptionHandlingMiddleware(RequestDelegate next, ILogger<McpExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (McpProtocolException ex)
        {
            _logger.LogWarning(ex,
                "MCP protocol error: {ErrorCode} - {Message}",
                ex.ErrorCode,
                ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = (int)ex.ErrorCode,
                    message = ex.Message
                }
            });
        }
        catch (Exception ex)
        {
            var sanitizedMethod = LogSanitizer.Sanitize(context.Request.Method);
            var sanitizedPath = LogSanitizer.Sanitize(context.Request.Path.Value);

            _logger.LogError(ex, "Unhandled exception processing request: {RequestMethod} {RequestPath}",
                sanitizedMethod,
                sanitizedPath);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = -32603,
                    message = "Internal server error"
                }
            });
        }
    }
}
