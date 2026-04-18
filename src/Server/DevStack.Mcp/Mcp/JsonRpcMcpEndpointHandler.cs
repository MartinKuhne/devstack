using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevStack.Mcp;

public class JsonRpcMcpEndpointHandler
{
    private readonly IMcpMethodHandler _methodHandler;
    private readonly ILogger<JsonRpcMcpEndpointHandler> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private static readonly ConcurrentDictionary<string, DateTime> _sessions = new();

    public JsonRpcMcpEndpointHandler(
        IMcpMethodHandler methodHandler,
        ILogger<JsonRpcMcpEndpointHandler> logger)
    {
        _methodHandler = methodHandler;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public async Task HandleMcpRequestAsync(HttpContext context)
    {
        try
        {
            var accept = context.Request.Headers.Accept.ToString();
            if (!string.IsNullOrEmpty(accept) && !accept.Contains("application/json") && !accept.Contains("*/*"))
            {
                context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                return;
            }

            if (!context.Request.ContentType?.Contains("application/json") ?? true)
            {
                await SendErrorResponseAsync(context, null, JsonRpcErrorCode.InvalidRequest,
                    "Content-Type must be application/json");
                return;
            }

            string requestBody;
            try
            {
                using var reader = new StreamReader(context.Request.Body);
                requestBody = await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read request body");
                await SendErrorResponseAsync(context, null, JsonRpcErrorCode.ParseError,
                    "Failed to read request body");
                return;
            }

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                await SendErrorResponseAsync(context, null, JsonRpcErrorCode.ParseError,
                    "Request body is empty");
                return;
            }

            JsonRpcRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<JsonRpcRequest>(requestBody, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse JSON request");
                await SendErrorResponseAsync(context, null, JsonRpcErrorCode.ParseError,
                    "Invalid JSON in request body");
                return;
            }

            if (request == null)
            {
                await SendErrorResponseAsync(context, null, JsonRpcErrorCode.InvalidRequest, "Request is null");
                return;
            }

            if (request.JsonRpc != "2.0")
            {
                await SendErrorResponseAsync(context, request.Id, JsonRpcErrorCode.InvalidRequest,
                    "jsonrpc must be '2.0'");
                return;
            }

            if (string.IsNullOrEmpty(request.Method))
            {
                await SendErrorResponseAsync(context, request.Id, JsonRpcErrorCode.InvalidRequest,
                    "method is required");
                return;
            }

            if (request.Id == null)
            {
                _logger.LogInformation("Received MCP notification: {Method}", request.Method);
                context.Response.StatusCode = StatusCodes.Status202Accepted;
                return;
            }

            try
            {
                _logger.LogInformation("Processing JSON-RPC method: {Method}", request.Method);
                var result = await _methodHandler.HandleAsync(request.Method, request.Params);
                var response = new JsonRpcResponse { Result = result, Id = request.Id };
                await SendResponseAsync(context, response, isInitialize: request.Method == "initialize");
            }
            catch (JsonRpcException ex)
            {
                _logger.LogWarning("JSON-RPC method error: {Code} - {Message}", ex.Code, ex.Message);
                await SendErrorResponseAsync(context, request.Id, ex.Code, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in JSON-RPC handler");
                await SendErrorResponseAsync(context, request.Id, JsonRpcErrorCode.InternalError,
                    "Internal server error");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in JSON-RPC endpoint");
            await SendErrorResponseAsync(context, null, JsonRpcErrorCode.InternalError, "Internal server error");
        }
    }

    public static async Task HandleSseStreamAsync(HttpContext context)
    {
        context.Response.ContentType = "text/event-stream";
        context.Response.Headers["Cache-Control"] = "no-cache";
        context.Response.Headers["X-Accel-Buffering"] = "no";

        await context.Response.WriteAsync(": connected\n\n");
        await context.Response.Body.FlushAsync();

        var tcs = new TaskCompletionSource();
        context.RequestAborted.Register(() => tcs.TrySetResult());
        await tcs.Task;
    }

    private async Task SendResponseAsync(HttpContext context, JsonRpcResponse response, bool isInitialize = false)
    {
        if (isInitialize)
        {
            var sessionId = Guid.NewGuid().ToString();
            _sessions[sessionId] = DateTime.UtcNow;
            context.Response.Headers["Mcp-Session-Id"] = sessionId;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status200OK;

        var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private async Task SendErrorResponseAsync(HttpContext context, JsonElement? id, int errorCode, string errorMessage)
    {
        var response = new JsonRpcResponse
        {
            Error = new JsonRpcError { Code = errorCode, Message = errorMessage },
            Id = id
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status200OK;

        var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}