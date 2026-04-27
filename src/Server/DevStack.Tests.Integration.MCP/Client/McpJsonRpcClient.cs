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

    public McpJsonRpcClient(HttpClient httpClient, string endpoint)
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
        var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PostAsync(_endpoint, content, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            throw new JsonException("Empty response from MCP server");
        }

        JsonRpcResponse[] jsonRpcResponses;

        // Try to parse as plain JSON first (for non-streaming responses)
        var trimmedContent = responseContent.Trim();
        if (trimmedContent.StartsWith("{"))
        {
            try
            {
                var response = JsonSerializer.Deserialize<JsonRpcResponse>(trimmedContent, _jsonOptions);
                if (response != null)
                {
                    jsonRpcResponses = new[] { response };
                }
                else
                {
                    jsonRpcResponses = ParseSseStream(responseContent);
                }
            }
            catch
            {
                jsonRpcResponses = ParseSseStream(responseContent);
            }
        }
        else
        {
            jsonRpcResponses = ParseSseStream(responseContent);
        }

        if (jsonRpcResponses.Length == 0)
        {
            throw new JsonException("No JSON-RPC responses received from MCP server");
        }

        var matchingResponse = jsonRpcResponses.FirstOrDefault(r => r.Id == request.Id);
        if (matchingResponse != null)
        {
            if (matchingResponse.Error != null)
            {
                throw new JsonRpcException(matchingResponse.Error.Code, matchingResponse.Error.Message, matchingResponse.Error.Data);
            }
            return matchingResponse;
        }

        var lastResponse = jsonRpcResponses[jsonRpcResponses.Length - 1];
        if (lastResponse.Error != null)
        {
            throw new JsonRpcException(lastResponse.Error.Code, lastResponse.Error.Message, lastResponse.Error.Data);
        }
        return lastResponse;
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
        var jsonContent = JsonSerializer.Serialize(requests, _jsonOptions);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PostAsync(_endpoint, content, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            throw new JsonException("Empty response from MCP server");
        }

        var jsonRpcResponses = ParseSseStream(responseContent);

        if (jsonRpcResponses.Length == 0)
        {
            throw new JsonException("No JSON-RPC responses received from MCP server");
        }

        foreach (var response in jsonRpcResponses)
        {
            if (response.Error != null)
            {
                throw new JsonRpcException(response.Error.Code, response.Error.Message, response.Error.Data);
            }
        }

        return jsonRpcResponses;
    }

    private static JsonRpcResponse[] ParseSseStream(string sseContent)
    {
        var responses = new List<JsonRpcResponse>();
        var lines = sseContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        var currentEvent = new StringBuilder();
        var currentEventType = "";

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (line.StartsWith("event:"))
            {
                var eventType = line.Substring(6).Trim();
                if (eventType == "message")
                {
                    currentEvent.Clear();
                    currentEventType = eventType;
                }
                else if (eventType == "error")
                {
                    currentEvent.Clear();
                    currentEventType = eventType;
                }
            }
            else if (line.StartsWith("data:"))
            {
                var dataLine = line.Substring(5).Trim();
                if (currentEvent.Length > 0 || dataLine.StartsWith("{"))
                {
                    currentEvent.AppendLine(dataLine);
                }
            }
            else if (line == "event: end" || line == "")
            {
                if (currentEvent.Length > 0)
                {
                    try
                    {
                        var response = JsonSerializer.Deserialize<JsonRpcResponse>(currentEvent.ToString(), new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (response != null)
                        {
                            responses.Add(response);
                        }
                    }
                    catch
                    {
                        // Ignore malformed JSON in SSE stream
                    }
                    currentEvent.Clear();
                }
                currentEventType = "";
            }
        }

        if (currentEvent.Length > 0)
        {
            try
            {
                var response = JsonSerializer.Deserialize<JsonRpcResponse>(currentEvent.ToString(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (response != null)
                {
                    responses.Add(response);
                }
            }
            catch
            {
                // Ignore malformed JSON in SSE stream
            }
        }

        return responses.ToArray();
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
