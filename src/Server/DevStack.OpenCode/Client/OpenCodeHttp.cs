using System.Text;

using DevStack.OpenCode.Serialization;

using Microsoft.Extensions.Logging;

namespace DevStack.OpenCode.Client;

/// <summary>
/// Internal HTTP helper shared by every OpenCode SDK sub-client. Centralizes
/// the JSON serialization options, query-string assembly, and error
/// reporting so that sub-clients stay focused on their domain.
/// </summary>
internal sealed class OpenCodeHttp
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;

    public OpenCodeHttp(HttpClient http, ILogger logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>Underlying <see cref="HttpClient"/>.</summary>
    public HttpClient Http => _http;

    /// <summary>Sends a GET request and deserializes the response body.</summary>
    public async Task<T> GetAsync<T>(string relativePath, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var body = await SendForStringAsync(HttpMethod.Get, BuildUri(relativePath, query), body: null, cancellationToken).ConfigureAwait(false);
        return Deserialize<T>(body, relativePath);
    }

    /// <summary>Sends a GET and returns the raw response body as text.</summary>
    public async Task<string> GetStringAsync(string relativePath, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        return await SendForStringAsync(HttpMethod.Get, BuildUri(relativePath, query), body: null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Sends a GET and returns <c>null</c> when the response is 404.</summary>
    public async Task<T?> GetNullableAsync<T>(string relativePath, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
        where T : class
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(relativePath, query));
        request.Headers.Accept.ParseAdd("application/json");
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, relativePath, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        return Deserialize<T>(body, relativePath);
    }

    /// <summary>Sends a GET that streams Server-Sent Events.</summary>
    public async Task<Stream> GetStreamAsync(string relativePath, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(relativePath, query));
        request.Headers.Accept.ParseAdd("text/event-stream");
        var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, relativePath, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Sends a POST with a JSON body and deserializes the response.</summary>
    public async Task<TResponse> PostAsync<TRequest, TResponse>(string relativePath, TRequest? body, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var json = body is null ? null : JsonSerializer.Serialize(body, OpenCodeJson.Compact);
        var response = await SendForStringAsync(HttpMethod.Post, BuildUri(relativePath, query), body: json, cancellationToken).ConfigureAwait(false);
        return Deserialize<TResponse>(response, relativePath);
    }

    /// <summary>Sends a POST that returns a raw boolean status.</summary>
    public async Task<bool> PostBoolAsync<TRequest>(string relativePath, TRequest? body, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var json = body is null ? null : JsonSerializer.Serialize(body, OpenCodeJson.Compact);
        var response = await SendForStringAsync(HttpMethod.Post, BuildUri(relativePath, query), body: json, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response))
        {
            return true;
        }

        if (bool.TryParse(response, out var b))
        {
            return b;
        }

        return true;
    }

    /// <summary>Sends a POST that returns no body.</summary>
    public async Task PostNoContentAsync<TRequest>(string relativePath, TRequest? body, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var json = body is null ? null : JsonSerializer.Serialize(body, OpenCodeJson.Compact);
        await SendForStringAsync(HttpMethod.Post, BuildUri(relativePath, query), body: json, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Sends a POST with a raw JSON document body.</summary>
    public async Task<TResponse> PostJsonAsync<TResponse>(string relativePath, JsonDocument body, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var json = body.RootElement.GetRawText();
        var response = await SendForStringAsync(HttpMethod.Post, BuildUri(relativePath, query), body: json, cancellationToken).ConfigureAwait(false);
        return Deserialize<TResponse>(response, relativePath);
    }

    /// <summary>Sends a PATCH with a JSON body and deserializes the response.</summary>
    public async Task<TResponse> PatchAsync<TRequest, TResponse>(string relativePath, TRequest? body, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var json = body is null ? null : JsonSerializer.Serialize(body, OpenCodeJson.Compact);
        var response = await SendForStringAsync(HttpMethod.Patch, BuildUri(relativePath, query), body: json, cancellationToken).ConfigureAwait(false);
        return Deserialize<TResponse>(response, relativePath);
    }

    /// <summary>Sends a PATCH with a raw JSON document body and deserializes the response.</summary>
    public async Task<TResponse> PatchAsync<TResponse>(string relativePath, JsonDocument body, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var json = body.RootElement.GetRawText();
        var response = await SendForStringAsync(HttpMethod.Patch, BuildUri(relativePath, query), body: json, cancellationToken).ConfigureAwait(false);
        return Deserialize<TResponse>(response, relativePath);
    }

    /// <summary>Sends a PUT with a JSON body and deserializes the response.</summary>
    public async Task<TResponse> PutAsync<TRequest, TResponse>(string relativePath, TRequest? body, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var json = body is null ? null : JsonSerializer.Serialize(body, OpenCodeJson.Compact);
        var response = await SendForStringAsync(HttpMethod.Put, BuildUri(relativePath, query), body: json, cancellationToken).ConfigureAwait(false);
        return Deserialize<TResponse>(response, relativePath);
    }

    /// <summary>Sends a PUT that returns a raw boolean status.</summary>
    public async Task<bool> PutBoolAsync<TRequest>(string relativePath, TRequest? body, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var json = body is null ? null : JsonSerializer.Serialize(body, OpenCodeJson.Compact);
        var response = await SendForStringAsync(HttpMethod.Put, BuildUri(relativePath, query), body: json, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response))
        {
            return true;
        }

        if (bool.TryParse(response, out var b))
        {
            return b;
        }

        return true;
    }

    /// <summary>Sends a DELETE. Returns <c>false</c> when the resource was not found.</summary>
    public async Task<bool> DeleteAsync(string relativePath, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, BuildUri(relativePath, query));
        request.Headers.Accept.ParseAdd("application/json");
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, relativePath, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>Sends a DELETE and deserializes the response body.</summary>
    public async Task<T> DeleteAsync<T>(string relativePath, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var body = await SendForStringAsync(HttpMethod.Delete, BuildUri(relativePath, query), body: null, cancellationToken).ConfigureAwait(false);
        return Deserialize<T>(body, relativePath);
    }

    /// <summary>Builds an absolute URI from a relative path and optional query string.</summary>
    public Uri BuildUri(string relativePath, IDictionary<string, string?>? query = null)
    {
        var baseUri = _http.BaseAddress?.ToString() ?? "http://localhost/";
        if (!baseUri.EndsWith('/'))
        {
            baseUri += "/";
        }

        var uri = new Uri(new Uri(baseUri, UriKind.Absolute), relativePath);
        if (query is null || query.Count == 0)
        {
            return uri;
        }

        var separator = uri.Query.Length == 0 ? "?" : "&";
        var sb = new StringBuilder(uri.ToString());
        foreach (var (key, value) in query)
        {
            if (value is null)
            {
                continue;
            }

            sb.Append(separator);
            sb.Append(Uri.EscapeDataString(key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(value));
            separator = "&";
        }

        return new Uri(sb.ToString(), UriKind.Absolute);
    }

    private async Task<string> SendForStringAsync(HttpMethod method, Uri uri, string? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        request.Headers.Accept.ParseAdd("application/json");

        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, uri.ToString(), cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, string operation, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var requestUri = response.RequestMessage?.RequestUri ?? new Uri(operation, UriKind.RelativeOrAbsolute);

        var (errorMessage, errorRef) = TryParseErrorEnvelope(body);

        _logger.LogError(
            "OpenCode request {Operation} failed: {Status} ref={Ref} message={Message} body={Body}",
            operation, (int)response.StatusCode, errorRef ?? "<none>", errorMessage ?? "<none>", body);

        var summary = errorMessage is null
            ? $"OpenCode request to {operation} failed with status {(int)response.StatusCode}."
            : $"OpenCode request to {operation} failed with status {(int)response.StatusCode}: {errorMessage}";

        if (errorRef is not null)
        {
            summary += $" (server ref: {errorRef})";
        }

        throw new OpenCodeRequestException(
            requestUri: requestUri,
            statusCode: (int)response.StatusCode,
            rawBody: body,
            errorMessage: errorMessage,
            errorRef: errorRef,
            message: summary);
    }

    /// <summary>
    /// Best-effort parse of the OpenCode server error envelope. The server
    /// returns either <c>{"name":"UnknownError","data":{"message":"...","ref":"err_..."}}</c>
    /// (5xx) or <c>{"name":"BadRequest","data":{"message":"...","kind":"Body"}}</c>
    /// (4xx). Anything else returns <c>(null, null)</c>.
    /// </summary>
    private static (string? Message, string? Ref) TryParseErrorEnvelope(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return (null, null);
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string? message = null;
            if (root.TryGetProperty("data", out var data)
                && data.ValueKind == JsonValueKind.Object
                && data.TryGetProperty("message", out var msg)
                && msg.ValueKind == JsonValueKind.String)
            {
                message = msg.GetString();
            }
            else if (root.TryGetProperty("message", out var rootMsg)
                     && rootMsg.ValueKind == JsonValueKind.String)
            {
                // Some plain-text-ish error responses just put the message at the root.
                message = rootMsg.GetString();
            }

            string? refId = null;
            if (root.TryGetProperty("ref", out var refProp) && refProp.ValueKind == JsonValueKind.String)
            {
                refId = refProp.GetString();
            }
            else if (data.ValueKind == JsonValueKind.Object
                     && data.TryGetProperty("ref", out var dataRef)
                     && dataRef.ValueKind == JsonValueKind.String)
            {
                refId = dataRef.GetString();
            }

            return (message, refId);
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    private static T Deserialize<T>(string body, string operation)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(body, OpenCodeJson.Compact)
                ?? throw new InvalidOperationException(
                    $"OpenCode response for {operation} was null.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize OpenCode response for {operation}.", ex);
        }
    }
}
