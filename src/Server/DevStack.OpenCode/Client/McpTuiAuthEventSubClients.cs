using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

internal sealed class McpClient : IMcpClient
{
    private readonly OpenCodeHttp _http;
    public McpClient(OpenCodeHttp http)
    {
        _http = http;
        Auth = new McpAuthClient(http);
    }

    public IMcpAuthClient Auth { get; }

    public async Task<IReadOnlyDictionary<string, McpStatus>> GetStatusAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<Dictionary<string, McpStatus>>("mcp", DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyDictionary<string, McpStatus>> AddAsync(McpAddRequest request, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.PostAsync<McpAddRequest, Dictionary<string, McpStatus>>("mcp", request, DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    public Task<bool> ConnectAsync(string name, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync<object?>($"mcp/{Uri.EscapeDataString(name)}/connect", null, DirectoryQuery(directory), cancellationToken);

    public Task<bool> DisconnectAsync(string name, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync<object?>($"mcp/{Uri.EscapeDataString(name)}/disconnect", null, DirectoryQuery(directory), cancellationToken);

    private static Dictionary<string, string?>? DirectoryQuery(string? directory) =>
        directory is null ? null : new() { ["directory"] = directory };
}

internal sealed class McpAuthClient : IMcpAuthClient
{
    private readonly OpenCodeHttp _http;
    public McpAuthClient(OpenCodeHttp http) => _http = http;

    public Task<bool> RemoveAsync(string name, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.DeleteAsync($"mcp/{Uri.EscapeDataString(name)}/auth", DirectoryQuery(directory), cancellationToken);

    public async Task<McpAuthStartResponse> StartAsync(string name, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.PostAsync<object?, McpAuthStartResponse>($"mcp/{Uri.EscapeDataString(name)}/auth", null, DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    public async Task<McpStatus> CallbackAsync(string name, string code, string? directory = null, CancellationToken cancellationToken = default)
    {
        return await _http.PostAsync<McpAuthCallbackRequest, McpStatus>(
            $"mcp/{Uri.EscapeDataString(name)}/auth/callback",
            new McpAuthCallbackRequest { Code = code },
            DirectoryQuery(directory),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<McpStatus> AuthenticateAsync(string name, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.PostAsync<object?, McpStatus>($"mcp/{Uri.EscapeDataString(name)}/auth/authenticate", null, DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    private sealed record McpAuthCallbackRequest
    {
        [JsonPropertyName("code")] public string Code { get; init; } = string.Empty;
    }

    private static Dictionary<string, string?>? DirectoryQuery(string? directory) =>
        directory is null ? null : new() { ["directory"] = directory };
}

internal sealed class TuiClient : ITuiClient
{
    private readonly OpenCodeHttp _http;
    public TuiClient(OpenCodeHttp http)
    {
        _http = http;
        Control = new TuiControlClient(http);
    }

    public ITuiControlClient Control { get; }

    public Task<bool> AppendPromptAsync(string text, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync("tui/append-prompt", new TuiAppendPromptRequest { Text = text }, DirectoryQuery(directory), cancellationToken);

    public Task<bool> OpenHelpAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync<object?>("tui/open-help", null, DirectoryQuery(directory), cancellationToken);

    public Task<bool> OpenSessionsAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync<object?>("tui/open-sessions", null, DirectoryQuery(directory), cancellationToken);

    public Task<bool> OpenThemesAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync<object?>("tui/open-themes", null, DirectoryQuery(directory), cancellationToken);

    public Task<bool> OpenModelsAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync<object?>("tui/open-models", null, DirectoryQuery(directory), cancellationToken);

    public Task<bool> SubmitPromptAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync<object?>("tui/submit-prompt", null, DirectoryQuery(directory), cancellationToken);

    public Task<bool> ClearPromptAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync<object?>("tui/clear-prompt", null, DirectoryQuery(directory), cancellationToken);

    public Task<bool> ExecuteCommandAsync(string command, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync("tui/execute-command", new TuiExecuteCommandRequest { Command = command }, DirectoryQuery(directory), cancellationToken);

    public Task<bool> ShowToastAsync(TuiToastRequest request, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync("tui/show-toast", request, DirectoryQuery(directory), cancellationToken);

    public Task<bool> PublishAsync(TuiPublishRequest request, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync("tui/publish", request, DirectoryQuery(directory), cancellationToken);

    private static Dictionary<string, string?>? DirectoryQuery(string? directory) =>
        directory is null ? null : new() { ["directory"] = directory };

    private sealed record TuiAppendPromptRequest
    {
        [JsonPropertyName("text")] public string Text { get; init; } = string.Empty;
    }

    private sealed record TuiExecuteCommandRequest
    {
        [JsonPropertyName("command")] public string Command { get; init; } = string.Empty;
    }
}

internal sealed class TuiControlClient : ITuiControlClient
{
    private readonly OpenCodeHttp _http;
    public TuiControlClient(OpenCodeHttp http) => _http = http;

    public Task<TuiControlRequest> NextAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        _http.GetAsync<TuiControlRequest>("tui/control/next", DirectoryQuery(directory), cancellationToken);

    public Task<bool> SubmitResponseAsync(JsonElement body, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync("tui/control/response", body, DirectoryQuery(directory), cancellationToken);

    private static Dictionary<string, string?>? DirectoryQuery(string? directory) =>
        directory is null ? null : new() { ["directory"] = directory };
}

internal sealed class AuthClient : IAuthClient
{
    private readonly OpenCodeHttp _http;
    public AuthClient(OpenCodeHttp http) => _http = http;

    public Task<bool> SetAsync(string id, Auth credentials, CancellationToken cancellationToken = default) =>
        _http.PutBoolAsync($"auth/{Uri.EscapeDataString(id)}", credentials, cancellationToken: cancellationToken);
}

internal sealed class EventClient : IEventClient
{
    private readonly OpenCodeHttp _http;
    public EventClient(OpenCodeHttp http) => _http = http;

    public async IAsyncEnumerable<SdkEvent> SubscribeAsync(string? directory, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var stream = await _http.GetStreamAsync("event", directory is null ? null : new Dictionary<string, string?> { ["directory"] = directory }, cancellationToken).ConfigureAwait(false);
        await foreach (var evt in SseStream.ReadAsync(stream, SdkEventReader.ReadPayloadOnly, cancellationToken).ConfigureAwait(false))
        {
            yield return evt;
        }
    }
}
