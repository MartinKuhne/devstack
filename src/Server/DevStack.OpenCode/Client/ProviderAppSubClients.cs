using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

internal sealed class ProviderClient : IProviderClient
{
    private readonly OpenCodeHttp _http;
    public ProviderClient(OpenCodeHttp http)
    {
        _http = http;
        OAuth = new ProviderOAuthClient(http);
    }

    public IProviderOAuthClient OAuth { get; }

    public async Task<ProviderListResponse> ListAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<ProviderListResponse>("provider", DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<ProviderAuthMethod>>> GetAuthMethodsAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        (IReadOnlyDictionary<string, IReadOnlyList<ProviderAuthMethod>>)await _http.GetAsync<Dictionary<string, List<ProviderAuthMethod>>>("provider/auth", DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    private static Dictionary<string, string?>? DirectoryQuery(string? directory) =>
        directory is null ? null : new() { ["directory"] = directory };
}

internal sealed class ProviderOAuthClient : IProviderOAuthClient
{
    private readonly OpenCodeHttp _http;
    public ProviderOAuthClient(OpenCodeHttp http) => _http = http;

    public Task<ProviderAuthAuthorization> AuthorizeAsync(string id, int method, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostAsync<ProviderOAuthRequest, ProviderAuthAuthorization>(
            $"provider/{Uri.EscapeDataString(id)}/oauth/authorize",
            new ProviderOAuthRequest { Method = method },
            DirectoryQuery(directory),
            cancellationToken);

    public Task<bool> CallbackAsync(string id, int method, string? code = null, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync(
            $"provider/{Uri.EscapeDataString(id)}/oauth/callback",
            new ProviderOAuthCallbackRequest { Method = method, Code = code },
            DirectoryQuery(directory),
            cancellationToken);

    private static Dictionary<string, string?>? DirectoryQuery(string? directory) =>
        directory is null ? null : new() { ["directory"] = directory };

    private sealed record ProviderOAuthRequest
    {
        [JsonPropertyName("method")] public int Method { get; init; }
    }

    private sealed record ProviderOAuthCallbackRequest
    {
        [JsonPropertyName("method")] public int Method { get; init; }
        [JsonPropertyName("code")] public string? Code { get; init; }
    }
}

internal sealed class AppClient : IAppClient
{
    private readonly OpenCodeHttp _http;
    public AppClient(OpenCodeHttp http) => _http = http;

    public Task<bool> LogAsync(AppLogRequest request, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync("log", request, DirectoryQuery(directory), cancellationToken);

    public async Task<IReadOnlyList<SdkAgent>> ListAgentsAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<SdkAgent>>("agent", DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    private static Dictionary<string, string?>? DirectoryQuery(string? directory) =>
        directory is null ? null : new() { ["directory"] = directory };
}
