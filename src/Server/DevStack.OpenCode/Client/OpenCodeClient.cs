using DevStack.OpenCode.Models;
using DevStack.OpenCode.Options;
using DevStack.OpenCode.Serialization;

using Microsoft.Extensions.Options;

namespace DevStack.OpenCode.Client;

/// <summary>
/// Default <see cref="IOpenCodeClient"/> implementation backed by an
/// <see cref="HttpClient"/>. The client is configured for JSON responses and
/// uses the centralized <see cref="OpenCodeJson"/> serializer options.
/// </summary>
public sealed class OpenCodeClient : IOpenCodeClient
{
    private readonly HttpClient _http;
    private readonly ILogger<OpenCodeClient> _logger;

    /// <summary>
    /// Creates a new <see cref="OpenCodeClient"/>.
    /// </summary>
    /// <param name="http">HTTP client to use for requests.</param>
    /// <param name="options">SDK options. Falls back to defaults when null.</param>
    /// <param name="logger">Optional logger.</param>
    public OpenCodeClient(
        HttpClient http,
        IOptions<OpenCodeOptions>? options = null,
        ILogger<OpenCodeClient>? logger = null)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _logger = logger ?? NullLogger<OpenCodeClient>.Instance;

        var opts = options?.Value ?? new OpenCodeOptions();
        _http.BaseAddress = opts.BaseUrl;
        _http.Timeout = opts.HttpTimeout;
        if (!string.IsNullOrEmpty(opts.UserAgent) && _http.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _http.DefaultRequestHeaders.Add("User-Agent", opts.UserAgent);
        }
    }

    /// <inheritdoc />
    public Uri SchemaUri
    {
        get
        {
            var baseUri = _http.BaseAddress ?? new Uri("https://opencode.ai/");
            return new Uri(baseUri, "config.json");
        }
    }

    /// <inheritdoc />
    public async Task<OpenCodeSchemaDocument> GetSchemaAsync(CancellationToken cancellationToken = default)
    {
        using var response = await GetSchemaResponseAsync(cancellationToken).ConfigureAwait(false);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var document = await JsonSerializer
            .DeserializeAsync<OpenCodeSchemaDocument>(stream, OpenCodeJson.Compact, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"OpenCode schema endpoint returned an empty body: {SchemaUri}");

        _logger.LogDebug("Fetched OpenCode schema from {Uri}", SchemaUri);
        return document;
    }

    /// <inheritdoc />
    public async Task<string> GetSchemaJsonAsync(CancellationToken cancellationToken = default)
    {
        using var response = await GetSchemaResponseAsync(cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> GetSchemaResponseAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, SchemaUri);
        request.Headers.Accept.ParseAdd("application/json");

        var response = await _http
            .SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError(
                "OpenCode schema request to {Uri} failed: {Status} {Body}",
                SchemaUri, (int)response.StatusCode, body);
            throw new HttpRequestException(
                $"OpenCode schema request to {SchemaUri} failed with status {(int)response.StatusCode}.",
                inner: null,
                statusCode: response.StatusCode);
        }

        return response;
    }
}
