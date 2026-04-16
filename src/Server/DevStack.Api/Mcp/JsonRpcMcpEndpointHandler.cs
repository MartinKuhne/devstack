using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevStack.Api.Mcp;

/// <summary>
/// JSON-RPC 2.0 endpoint handler for MCP
/// Implements the specification from https://www.jsonrpc.org/specification
/// </summary>
public class JsonRpcMcpEndpointHandler
{
    private readonly IMcpMethodHandler _methodHandler;
    private readonly ILogger<JsonRpcMcpEndpointHandler> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

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
            // Validate content type
            if (!context.Request.ContentType?.Contains("application/json") ?? true)
            {
                await SendErrorResponseAsync(context, null, JsonRpcErrorCode.InvalidRequest,
                    "Content-Type must be application/json");
                return;
            }

            // Read and parse request body
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

            // Parse JSON-RPC request
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

            // Validate request
            if (request == null)
            {
                await SendErrorResponseAsync(context, null, JsonRpcErrorCode.InvalidRequest,
                    "Request is null");
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

            // Process request
            try
            {
                _logger.LogInformation("Processing JSON-RPC method: {Method}", request.Method);
                var result = await _methodHandler.HandleAsync(request.Method, request.Params);

                // Send success response
                var response = new JsonRpcResponse
                {
                    Result = result,
                    Id = request.Id
                };

                await SendResponseAsync(context, response);
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
            await SendErrorResponseAsync(context, null, JsonRpcErrorCode.InternalError,
                "Internal server error");
        }
    }

    private async Task SendResponseAsync(HttpContext context, JsonRpcResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status200OK;

        var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private async Task SendErrorResponseAsync(HttpContext context, JsonElement? id, int errorCode,
        string errorMessage)
    {
        var response = new JsonRpcResponse
        {
            Error = new JsonRpcError
            {
                Code = errorCode,
                Message = errorMessage
            },
            Id = id
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status200OK; // JSON-RPC always returns 200 for valid requests

        var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}
