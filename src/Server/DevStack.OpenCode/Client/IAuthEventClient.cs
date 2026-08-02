using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

/// <summary>Authentication operations (<c>client.auth.*</c>).</summary>
public interface IAuthClient
{
    /// <summary>Set authentication credentials (<c>PUT /auth/{id}</c>).</summary>
    Task<bool> SetAsync(string id, Auth credentials, CancellationToken cancellationToken = default);
}

/// <summary>Event subscription operations (<c>client.event.*</c>).</summary>
public interface IEventClient
{
    /// <summary>Server-sent events stream (<c>GET /event</c>).</summary>
    IAsyncEnumerable<SdkEvent> SubscribeAsync(string? directory = null, CancellationToken cancellationToken = default);
}
