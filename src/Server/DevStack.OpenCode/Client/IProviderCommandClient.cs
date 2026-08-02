using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

/// <summary>Provider operations (<c>client.provider.*</c>).</summary>
public interface IProviderClient
{
    /// <summary>List all providers (<c>GET /provider</c>).</summary>
    Task<ProviderListResponse> ListAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Get provider authentication methods (<c>GET /provider/auth</c>).</summary>
    Task<IReadOnlyDictionary<string, IReadOnlyList<ProviderAuthMethod>>> GetAuthMethodsAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Sub-client for OAuth flows.</summary>
    IProviderOAuthClient OAuth { get; }
}

/// <summary>Provider OAuth flows (<c>client.provider.oauth.*</c>).</summary>
public interface IProviderOAuthClient
{
    /// <summary>Authorize a provider using OAuth (<c>POST /provider/{id}/oauth/authorize</c>).</summary>
    Task<ProviderAuthAuthorization> AuthorizeAsync(string id, int method, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Handle OAuth callback for a provider (<c>POST /provider/{id}/oauth/callback</c>).</summary>
    Task<bool> CallbackAsync(string id, int method, string? code = null, string? directory = null, CancellationToken cancellationToken = default);
}

/// <summary>App-level operations (<c>client.app.*</c>).</summary>
public interface IAppClient
{
    /// <summary>Write a log entry to the server logs (<c>POST /log</c>).</summary>
    Task<bool> LogAsync(AppLogRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>List all agents (<c>GET /agent</c>).</summary>
    Task<IReadOnlyList<SdkAgent>> ListAgentsAsync(string? directory = null, CancellationToken cancellationToken = default);
}

/// <summary>Request body for <c>POST /log</c>.</summary>
public sealed record AppLogRequest
{
    /// <summary>Service name for the log entry.</summary>
    [JsonPropertyName("service")] public string Service { get; init; } = string.Empty;
    /// <summary>Log level.</summary>
    [JsonPropertyName("level")] public string Level { get; init; } = "info";
    /// <summary>Log message.</summary>
    [JsonPropertyName("message")] public string Message { get; init; } = string.Empty;
    /// <summary>Additional metadata.</summary>
    [JsonPropertyName("extra")] public IDictionary<string, JsonElement>? Extra { get; init; }
}

/// <summary>Command operations (<c>client.command.*</c>).</summary>
public interface ICommandClient
{
    /// <summary>List all commands (<c>GET /command</c>).</summary>
    Task<IReadOnlyList<SdkCommand>> ListAsync(string? directory = null, CancellationToken cancellationToken = default);
}
