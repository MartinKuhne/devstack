using System.Diagnostics;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace DevStack.Mcp.Logging;

public static class McpToolLoggingFilter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false
    };

    /// <summary>Creates a call-tool filter that logs invocation outcomes.</summary>
    public static McpRequestFilter<CallToolRequestParams, CallToolResult> Create()
    {
        return next => async (request, ct) =>
        {
            var logger = request.Services?.GetService<ILoggerFactory>()
                ?.CreateLogger("DevStack.Mcp.Tools") ?? NullLogger.Instance;

            // OWASP CWE-117: Sanitize user-provided MCP tool name before logging or adding to structured scope
            var toolName = LogSanitizer.Sanitize(request.Params?.Name ?? "unknown");
            var arguments = request.Params?.Arguments;
            var stopwatch = Stopwatch.StartNew();

            using (logger.BeginScope(new Dictionary<string, object>
            {
                ["ToolName"] = toolName,
                ["CorrelationId"] = Activity.Current?.TraceId.ToString() ?? "none"
            }))
            {
                logger.LogInformation("Tool invocation started: {ToolName} with arguments: {Arguments}",
                    toolName,
                    arguments is not null ? JsonSerializer.Serialize(arguments, SerializerOptions) : "null");

                try
                {
                    var result = await next(request, ct);
                    if (result is null)
                    {
                        throw new InvalidOperationException("The MCP tool handler returned no result.");
                    }

                    stopwatch.Stop();
                    var isError = result.IsError == true;
                    var logLevel = isError ? LogLevel.Warning : LogLevel.Information;

                    logger.Log(logLevel,
                        "Tool invocation completed: {ToolName} in {ElapsedMs}ms with isError={IsError}",
                        toolName,
                        stopwatch.ElapsedMilliseconds,
                        isError);

                    return result;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    logger.LogError(ex,
                        "Tool invocation failed: {ToolName} in {ElapsedMs}ms with error: {ErrorMessage}",
                        toolName,
                        stopwatch.ElapsedMilliseconds,
                        ex.Message);
                    throw;
                }
            }
        };
    }
}
