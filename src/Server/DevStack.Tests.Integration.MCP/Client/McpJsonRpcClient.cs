using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace DevStack.Tests.Integration.MCP.Client;

public class McpJsonRpcClient : IMcpJsonRpcClient
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly JsonSerializerOptions _jsonOptions;
    private int _requestId;

    public McpJsonRpcClient(HttpClient httpClient, string endpoint = "http://localhost:8887/mcp")
    {
        _httpClient = httpClient;
        _endpoint = endpoint;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<JsonRpcResponse> SendRequestAsync<TParams>(string method, TParams? @params, CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest("2.0", method, @params, Interlocked.Increment(ref _requestId));
        return await SendPrivateRequestAsync(request, cancellationToken);
    }

    public async Task<JsonRpcResponse> SendRequestAsync(string method, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<object>(method, null!, cancellationToken);
    }

    public async Task SendNotificationAsync<TParams>(string method, TParams? @params, CancellationToken cancellationToken = default)
    {
        var notification = new JsonRpcNotification("2.0", method, @params);
        await SendPrivateNotificationAsync(notification, cancellationToken);
    }

    public async Task<JsonRpcResponse[]> SendBatchRequestAsync(JsonRpcRequest[] requests, CancellationToken cancellationToken = default)
    {
        return await SendPrivateBatchRequestAsync(requests, cancellationToken);
    }

    private async Task<JsonRpcResponse> SendPrivateRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(request, _jsonOptions),
            Encoding.UTF8,
            "application/json"
        );

        var httpResponse = await _httpClient.PostAsync(_endpoint, content, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var response = JsonSerializer.Deserialize<JsonRpcResponse>(responseContent, _jsonOptions)
            ?? throw new JsonException("Failed to deserialize JSON-RPC response");

        if (response.Error != null)
        {
            throw new JsonRpcException(response.Error.Code, response.Error.Message, response.Error.Data);
        }

        return response;
    }

    private async Task SendPrivateNotificationAsync(JsonRpcNotification notification, CancellationToken cancellationToken)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(notification, _jsonOptions),
            Encoding.UTF8,
            "application/json"
        );

        await _httpClient.PostAsync(_endpoint, content, cancellationToken);
    }

    private async Task<JsonRpcResponse[]> SendPrivateBatchRequestAsync(JsonRpcRequest[] requests, CancellationToken cancellationToken)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(requests, _jsonOptions),
            Encoding.UTF8,
            "application/json"
        );

        var httpResponse = await _httpClient.PostAsync(_endpoint, content, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var responses = JsonSerializer.Deserialize<JsonRpcResponse[]>(responseContent, _jsonOptions)
            ?? throw new JsonException("Failed to deserialize JSON-RPC batch response");

        foreach (var response in responses)
        {
            if (response.Error != null)
            {
                throw new JsonRpcException(response.Error.Code, response.Error.Message, response.Error.Data);
            }
        }

        return responses;
    }
}

public class JsonRpcException : Exception
{
    public int Code { get; }
    public new object? Data { get; }

    public JsonRpcException(int code, string message, object? data = null)
        : base(message)
    {
        Code = code;
        Data = data;
    }
}
