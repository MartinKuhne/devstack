using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

/// <summary>Config operations (<c>client.config.*</c>).</summary>
public interface IConfigClient
{
    /// <summary>Get the full configuration (<c>GET /config</c>).</summary>
    Task<OpenCodeConfig> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Update the configuration (<c>PATCH /config</c>).</summary>
    Task<OpenCodeConfig> UpdateAsync(OpenCodeConfig config, CancellationToken cancellationToken = default);

    /// <summary>Merge a JSON Patch document into the configuration.</summary>
    Task<OpenCodeConfig> PatchAsync(JsonDocument patch, CancellationToken cancellationToken = default);

    /// <summary>List providers and default models (<c>GET /config/providers</c>).</summary>
    Task<ConfigProvidersResponse> GetProvidersAsync(CancellationToken cancellationToken = default);
}

/// <summary>LSP status (<c>client.lsp.*</c>).</summary>
public interface ILspClient
{
    /// <summary>Get LSP server status (<c>GET /lsp</c>).</summary>
    Task<IReadOnlyList<SdkLspStatus>> GetStatusAsync(string? directory = null, CancellationToken cancellationToken = default);
}

/// <summary>Formatter status (<c>client.formatter.*</c>).</summary>
public interface IFormatterClient
{
    /// <summary>Get formatter status (<c>GET /formatter</c>).</summary>
    Task<IReadOnlyList<SdkFormatterStatus>> GetStatusAsync(string? directory = null, CancellationToken cancellationToken = default);
}
