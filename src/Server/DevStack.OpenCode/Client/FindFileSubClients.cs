using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

internal sealed class FindClient : IFindClient
{
    private readonly OpenCodeHttp _http;
    public FindClient(OpenCodeHttp http) => _http = http;

    public async Task<IReadOnlyList<TextMatch>> FindTextAsync(string pattern, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<TextMatch>>("find", BuildQuery(pattern: pattern, directory: directory), cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<string>> FindFilesAsync(string query, string? directory = null, string? type = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var q = BuildQuery(query: query, directory: directory, type: type, limit: limit);
        return await _http.GetAsync<List<string>>("find/file", q, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Symbol>> FindSymbolsAsync(string query, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<Symbol>>("find/symbol", BuildQuery(query: query, directory: directory), cancellationToken).ConfigureAwait(false);

    private static Dictionary<string, string?> BuildQuery(string? pattern = null, string? query = null, string? directory = null, string? type = null, int? limit = null)
    {
        var result = new Dictionary<string, string?>();
        if (pattern is not null) result["pattern"] = pattern;
        if (query is not null) result["query"] = query;
        if (directory is not null) result["directory"] = directory;
        if (type is not null) result["type"] = type;
        if (limit.HasValue) result["limit"] = limit.Value.ToString();
        return result;
    }
}

internal sealed class FileClient : IFileClient
{
    private readonly OpenCodeHttp _http;
    public FileClient(OpenCodeHttp http) => _http = http;

    public async Task<IReadOnlyList<FileNode>> ListAsync(string path, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<FileNode>>("file", BuildQuery(path: path, directory: directory), cancellationToken).ConfigureAwait(false);

    public async Task<FileContent> ReadAsync(string path, string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<FileContent>("file/content", BuildQuery(path: path, directory: directory), cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<SdkFile>> GetStatusAsync(string? directory = null, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<SdkFile>>("file/status", directory is null ? null : new Dictionary<string, string?> { ["directory"] = directory }, cancellationToken).ConfigureAwait(false);

    private static Dictionary<string, string?> BuildQuery(string? path = null, string? directory = null)
    {
        var result = new Dictionary<string, string?>();
        if (path is not null) result["path"] = path;
        if (directory is not null) result["directory"] = directory;
        return result;
    }
}
