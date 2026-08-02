using System.Text.Json;

using Microsoft.Extensions.Logging;

using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace DevStack.Mcp.Logging;

public static class McpArgumentNormalizationFilter
{
    public static McpRequestFilter<CallToolRequestParams, CallToolResult> Create(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DevStack.Mcp.ArgumentNormalization");

        return next => async (request, ct) =>
        {
            var toolName = LogSanitizer.Sanitize(request.Params?.Name ?? "unknown");
            var arguments = request.Params?.Arguments;

            if (arguments is not null)
            {
                NormalizeArguments(arguments, toolName, logger);
            }

            return await next(request, ct);
        };
    }

    internal static void NormalizeArguments(IDictionary<string, JsonElement> arguments, string toolName, Microsoft.Extensions.Logging.ILogger logger)
    {
        var keys = arguments.Keys.ToList();

        foreach (var key in keys)
        {
            var value = arguments[key];

            if (value.ValueKind == JsonValueKind.Array)
            {
                var elements = new List<string>();

                foreach (var element in value.EnumerateArray())
                {
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        elements.Add(element.GetString() ?? string.Empty);
                    }
                    else if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                    {
                        elements.Add(string.Empty);
                    }
                    else
                    {
                        elements.Add(element.GetRawText());
                    }
                }

                var joined = string.Join("\n", elements);
                var normalizedElement = JsonSerializer.SerializeToElement(joined);
                arguments[key] = normalizedElement;

                logger.LogWarning(
                    "Tool {ToolName}: Coerced array argument for parameter '{Parameter}' to string",
                    toolName,
                    key);
            }
        }
    }
}
