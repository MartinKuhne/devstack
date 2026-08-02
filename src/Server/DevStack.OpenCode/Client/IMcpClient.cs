using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

/// <summary>MCP server operations (<c>client.mcp.*</c>).</summary>
public interface IMcpClient
{
    /// <summary>Get MCP server status (<c>GET /mcp</c>).</summary>
    Task<IReadOnlyDictionary<string, McpStatus>> GetStatusAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Add MCP server dynamically (<c>POST /mcp</c>).</summary>
    Task<IReadOnlyDictionary<string, McpStatus>> AddAsync(McpAddRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Connect an MCP server (<c>POST /mcp/{name}/connect</c>).</summary>
    Task<bool> ConnectAsync(string name, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Disconnect an MCP server (<c>POST /mcp/{name}/disconnect</c>).</summary>
    Task<bool> DisconnectAsync(string name, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Sub-client for OAuth flows.</summary>
    IMcpAuthClient Auth { get; }
}

/// <summary>Sub-client for MCP OAuth flows (<c>client.mcp.auth.*</c>).</summary>
public interface IMcpAuthClient
{
    /// <summary>Remove OAuth credentials for an MCP server (<c>DELETE /mcp/{name}/auth</c>).</summary>
    Task<bool> RemoveAsync(string name, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Start OAuth authentication flow for an MCP server (<c>POST /mcp/{name}/auth</c>).</summary>
    Task<McpAuthStartResponse> StartAsync(string name, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Complete OAuth authentication with authorization code (<c>POST /mcp/{name}/auth/callback</c>).</summary>
    Task<McpStatus> CallbackAsync(string name, string code, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Start OAuth flow and wait for callback (opens browser) (<c>POST /mcp/{name}/auth/authenticate</c>).</summary>
    Task<McpStatus> AuthenticateAsync(string name, string? directory = null, CancellationToken cancellationToken = default);
}

/// <summary>Request body for <c>POST /mcp</c>.</summary>
public sealed record McpAddRequest
{
    /// <summary>Name of the new MCP server.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    /// <summary>Configuration for the new MCP server.</summary>
    [JsonPropertyName("config")] public McpServerConfig Config { get; init; } = McpServerConfig.FromToggle(new McpEnableToggle { Enabled = true });
}

/// <summary>Response from <c>POST /mcp/{name}/auth</c>.</summary>
public sealed record McpAuthStartResponse
{
    /// <summary>URL to open in the browser for authorization.</summary>
    [JsonPropertyName("authorizationUrl")] public string AuthorizationUrl { get; init; } = string.Empty;
}
