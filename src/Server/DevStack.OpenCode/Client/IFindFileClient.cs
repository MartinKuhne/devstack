using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

/// <summary>Find operations (<c>client.find.*</c>).</summary>
public interface IFindClient
{
    /// <summary>Search for text in files (<c>GET /find</c>).</summary>
    Task<IReadOnlyList<TextMatch>> FindTextAsync(string pattern, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Find files and directories by name (<c>GET /find/file</c>).</summary>
    Task<IReadOnlyList<string>> FindFilesAsync(string query, string? directory = null, string? type = null, int? limit = null, CancellationToken cancellationToken = default);

    /// <summary>Find workspace symbols (<c>GET /find/symbol</c>).</summary>
    Task<IReadOnlyList<Symbol>> FindSymbolsAsync(string query, string? directory = null, CancellationToken cancellationToken = default);
}

/// <summary>File operations (<c>client.file.*</c>).</summary>
public interface IFileClient
{
    /// <summary>List files and directories (<c>GET /file</c>).</summary>
    Task<IReadOnlyList<FileNode>> ListAsync(string path, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Read a file (<c>GET /file/content</c>).</summary>
    Task<FileContent> ReadAsync(string path, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Get status for tracked files (<c>GET /file/status</c>).</summary>
    Task<IReadOnlyList<SdkFile>> GetStatusAsync(string? directory = null, CancellationToken cancellationToken = default);
}
