using System.Runtime.CompilerServices;
using System.Text;

using DevStack.OpenCode.Models;

using Microsoft.Extensions.Logging;

namespace DevStack.OpenCode.Client;

internal sealed class ConfigClient : IConfigClient
{
    private readonly OpenCodeHttp _http;
    public ConfigClient(OpenCodeHttp http) => _http = http;

    public Task<OpenCodeConfig> GetAsync(CancellationToken cancellationToken = default) =>
        _http.GetAsync<OpenCodeConfig>("config", cancellationToken: cancellationToken);

    public Task<OpenCodeConfig> UpdateAsync(OpenCodeConfig config, CancellationToken cancellationToken = default) =>
        _http.PatchAsync<OpenCodeConfig, OpenCodeConfig>("config", config, cancellationToken: cancellationToken);

    public Task<OpenCodeConfig> PatchAsync(JsonDocument patch, CancellationToken cancellationToken = default) =>
        _http.PatchAsync<OpenCodeConfig>("config", patch, cancellationToken: cancellationToken);

    public Task<ConfigProvidersResponse> GetProvidersAsync(CancellationToken cancellationToken = default) =>
        _http.GetAsync<ConfigProvidersResponse>("config/providers", cancellationToken: cancellationToken);
}

internal sealed class ProjectClient : IProjectClient
{
    private readonly OpenCodeHttp _http;
    public ProjectClient(OpenCodeHttp http) => _http = http;

    public async Task<IReadOnlyList<SdkProject>> ListAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<SdkProject>>("project", BuildQuery(directory), cancellationToken).ConfigureAwait(false);

    public async Task<SdkProject> GetCurrentAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<SdkProject>("project/current", BuildQuery(directory), cancellationToken).ConfigureAwait(false);

    private static Dictionary<string, string?>? BuildQuery(string? directory) =>
        directory is null ? null : new() { ["directory"] = directory };
}

internal sealed class PtyClient : IPtyClient
{
    private readonly OpenCodeHttp _http;
    public PtyClient(OpenCodeHttp http) => _http = http;

    public async Task<IReadOnlyList<Pty>> ListAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<Pty>>("pty", DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    public async Task<Pty> CreateAsync(PtyCreateRequest? request = null, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.PostAsync<PtyCreateRequest?, Pty>("pty", request, DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    public Task<bool> RemoveAsync(string id, string? directory = null, CancellationToken cancellationToken = default) =>
        _http.DeleteAsync($"pty/{Uri.EscapeDataString(id)}", DirectoryQuery(directory), cancellationToken);

    public async Task<Pty> GetAsync(string id, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<Pty>($"pty/{Uri.EscapeDataString(id)}", DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    public async Task<Pty> UpdateAsync(string id, PtyUpdateRequest request, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.PutAsync<PtyUpdateRequest, Pty>($"pty/{Uri.EscapeDataString(id)}", request, DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    public async Task ConnectAsync(string id, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetStringAsync($"pty/{Uri.EscapeDataString(id)}/connect", DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    private static Dictionary<string, string?>? DirectoryQuery(string? directory) =>
        directory is null ? null : new() { ["directory"] = directory };
}

internal sealed class InstanceClient : IInstanceClient
{
    private readonly OpenCodeHttp _http;
    public InstanceClient(OpenCodeHttp http) => _http = http;

    public Task<bool> DisposeAsync(CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync<object?>("instance/dispose", null, cancellationToken: cancellationToken);
}

internal sealed class PathClient : IPathClient
{
    private readonly OpenCodeHttp _http;
    public PathClient(OpenCodeHttp http) => _http = http;

    public Task<ServerPath> GetAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        _http.GetAsync<ServerPath>("path", directory is null ? null : new Dictionary<string, string?> { ["directory"] = directory }, cancellationToken);
}

internal sealed class VcsClient : IVcsClient
{
    private readonly OpenCodeHttp _http;
    public VcsClient(OpenCodeHttp http) => _http = http;

    public Task<SdkVcsInfo> GetAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        _http.GetAsync<SdkVcsInfo>("vcs", directory is null ? null : new Dictionary<string, string?> { ["directory"] = directory }, cancellationToken);
}

internal sealed class GlobalClient : IGlobalClient
{
    private readonly OpenCodeHttp _http;
    public GlobalClient(OpenCodeHttp http) => _http = http;

    public async IAsyncEnumerable<GlobalEvent> SubscribeAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var stream = await _http.GetStreamAsync("global/event", cancellationToken: cancellationToken).ConfigureAwait(false);
        await foreach (var evt in SseStream.ReadAsync(stream, (root) => SdkEventReader.ReadGlobal(root), cancellationToken).ConfigureAwait(false))
        {
            yield return evt;
        }
    }
}

internal sealed class ToolClient : IToolClient
{
    private readonly OpenCodeHttp _http;
    public ToolClient(OpenCodeHttp http) => _http = http;

    public async Task<IReadOnlyList<string>> GetIdsAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<string>>("experimental/tool/ids", DirectoryQuery(directory), cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<ToolListItem>> ListAsync(string providerId, string modelId, string? directory = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["providerID"] = providerId,
            ["modelID"] = modelId,
        };
        if (directory is not null)
        {
            query["directory"] = directory;
        }

        return await _http.GetAsync<List<ToolListItem>>("experimental/tool", query, cancellationToken).ConfigureAwait(false);
    }

    private static Dictionary<string, string?>? DirectoryQuery(string? directory) =>
        directory is null ? null : new() { ["directory"] = directory };
}

internal sealed class LspClient : ILspClient
{
    private readonly OpenCodeHttp _http;
    public LspClient(OpenCodeHttp http) => _http = http;

    public async Task<IReadOnlyList<SdkLspStatus>> GetStatusAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<SdkLspStatus>>("lsp", directory is null ? null : new Dictionary<string, string?> { ["directory"] = directory }, cancellationToken).ConfigureAwait(false);
}

internal sealed class FormatterClient : IFormatterClient
{
    private readonly OpenCodeHttp _http;
    public FormatterClient(OpenCodeHttp http) => _http = http;

    public async Task<IReadOnlyList<SdkFormatterStatus>> GetStatusAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<SdkFormatterStatus>>("formatter", directory is null ? null : new Dictionary<string, string?> { ["directory"] = directory }, cancellationToken).ConfigureAwait(false);
}

internal sealed class CommandClient : ICommandClient
{
    private readonly OpenCodeHttp _http;
    public CommandClient(OpenCodeHttp http) => _http = http;

    public async Task<IReadOnlyList<SdkCommand>> ListAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<SdkCommand>>("command", directory is null ? null : new Dictionary<string, string?> { ["directory"] = directory }, cancellationToken).ConfigureAwait(false);
}

/// <summary>Helper for reading Server-Sent Events streams.</summary>
internal static class SseStream
{
    public static async IAsyncEnumerable<T> ReadAsync<T>(
        Stream stream,
        Func<JsonElement, T> reader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var sr = new StreamReader(stream, Encoding.UTF8);
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await sr.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
            {
                yield break;
            }

            if (line.StartsWith("data: ", StringComparison.Ordinal))
            {
                var payload = line["data: ".Length..];
                JsonDocument doc;
                try
                {
                    doc = JsonDocument.Parse(payload);
                }
                catch (JsonException)
                {
                    continue;
                }

                using (doc)
                {
                    yield return reader(doc.RootElement.Clone());
                }
            }
        }
    }
}

/// <summary>Helper for parsing event payloads.</summary>
internal static class SdkEventReader
{
    public static GlobalEvent ReadGlobal(JsonElement root)
    {
        var directory = root.TryGetProperty("directory", out var d) ? d.GetString() ?? string.Empty : string.Empty;
        var payload = root.TryGetProperty("payload", out var p)
            ? Read(p)
            : new SdkEvent("unknown", default);
        return new GlobalEvent { Directory = directory, Payload = payload };
    }

    public static SdkEvent ReadPayloadOnly(JsonElement root) => Read(root);

    private static SdkEvent Read(JsonElement root)
    {
        var type = root.TryGetProperty("type", out var t) ? t.GetString() ?? string.Empty : string.Empty;
        return new SdkEvent(type, root.Clone());
    }
}
