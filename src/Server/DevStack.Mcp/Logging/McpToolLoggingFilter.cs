using System.Diagnostics;
using System.Text.Json;

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

    public static McpRequestFilter<CallToolRequestParams, CallToolResult> Create(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DevStack.Mcp.Tools");

        return next => async (request, ct) =>
        {
            var toolName = request.Params?.Name ?? "unknown";
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
                    stopwatch.Stop();

                    var isError = result?.IsError == true;
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
