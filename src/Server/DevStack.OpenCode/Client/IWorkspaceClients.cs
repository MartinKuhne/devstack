using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

/// <summary>Project operations (<c>client.project.*</c>).</summary>
public interface IProjectClient
{
    /// <summary>List all projects (<c>GET /project</c>).</summary>
    Task<IReadOnlyList<SdkProject>> ListAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Get the current project (<c>GET /project/current</c>).</summary>
    Task<SdkProject> GetCurrentAsync(string? directory = null, CancellationToken cancellationToken = default);
}

/// <summary>PTY session operations (<c>client.pty.*</c>).</summary>
public interface IPtyClient
{
    /// <summary>List all PTY sessions (<c>GET /pty</c>).</summary>
    Task<IReadOnlyList<Pty>> ListAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Create a new PTY session (<c>POST /pty</c>).</summary>
    Task<Pty> CreateAsync(PtyCreateRequest? request = null, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Remove a PTY session (<c>DELETE /pty/{id}</c>).</summary>
    Task<bool> RemoveAsync(string id, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Get PTY session info (<c>GET /pty/{id}</c>).</summary>
    Task<Pty> GetAsync(string id, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Update PTY session (<c>PUT /pty/{id}</c>).</summary>
    Task<Pty> UpdateAsync(string id, PtyUpdateRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Connect to a PTY session (<c>GET /pty/{id}/connect</c>).</summary>
    Task ConnectAsync(string id, string? directory = null, CancellationToken cancellationToken = default);
}

/// <summary>Request body for <c>POST /pty</c>.</summary>
public sealed record PtyCreateRequest
{
    /// <summary>Shell command to run.</summary>
    [JsonPropertyName("command")] public string? Command { get; init; }
    /// <summary>Arguments to the command.</summary>
    [JsonPropertyName("args")] public IReadOnlyList<string>? Args { get; init; }
    /// <summary>Working directory.</summary>
    [JsonPropertyName("cwd")] public string? Cwd { get; init; }
    /// <summary>Display title for the PTY.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }
    /// <summary>Environment variables.</summary>
    [JsonPropertyName("env")] public IDictionary<string, string>? Env { get; init; }
}

/// <summary>Request body for <c>PUT /pty/{id}</c>.</summary>
public sealed record PtyUpdateRequest
{
    /// <summary>Display title for the PTY.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }
    /// <summary>Environment variables.</summary>
    [JsonPropertyName("env")] public IDictionary<string, string>? Env { get; init; }
}

/// <summary>Instance operations (<c>client.instance.*</c>).</summary>
public interface IInstanceClient
{
    /// <summary>Dispose the current instance (<c>POST /instance/dispose</c>).</summary>
    Task<bool> DisposeAsync(CancellationToken cancellationToken = default);
}

/// <summary>Path operations (<c>client.path.*</c>).</summary>
public interface IPathClient
{
    /// <summary>Get the current path (<c>GET /path</c>).</summary>
    Task<ServerPath> GetAsync(string? directory = null, CancellationToken cancellationToken = default);
}

/// <summary>VCS operations (<c>client.vcs.*</c>).</summary>
public interface IVcsClient
{
    /// <summary>Get VCS info for the current instance (<c>GET /vcs</c>).</summary>
    Task<SdkVcsInfo> GetAsync(string? directory = null, CancellationToken cancellationToken = default);
}

/// <summary>Global operations (<c>client.global.*</c>).</summary>
public interface IGlobalClient
{
    /// <summary>Server-sent events stream (<c>GET /global/event</c>).</summary>
    IAsyncEnumerable<GlobalEvent> SubscribeAsync(CancellationToken cancellationToken = default);
}

/// <summary>Tool operations (<c>client.tool.*</c>).</summary>
public interface IToolClient
{
    /// <summary>List all tool IDs (<c>GET /experimental/tool/ids</c>).</summary>
    Task<IReadOnlyList<string>> GetIdsAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>List tools with JSON schema parameters for a provider/model (<c>GET /experimental/tool</c>).</summary>
    Task<IReadOnlyList<ToolListItem>> ListAsync(string providerId, string modelId, string? directory = null, CancellationToken cancellationToken = default);
}
