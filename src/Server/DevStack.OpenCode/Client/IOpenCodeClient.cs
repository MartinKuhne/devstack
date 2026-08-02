using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

/// <summary>
/// HTTP client for the OpenCode schema endpoint. Implementations are
/// expected to be safe to register as a singleton.
/// </summary>
public interface IOpenCodeClient
{
    /// <summary>Fetches the raw JSON schema document.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The schema document returned by the server.</returns>
    Task<OpenCodeSchemaDocument> GetSchemaAsync(CancellationToken cancellationToken = default);

    /// <summary>Fetches the raw JSON schema document as text.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The raw JSON text returned by the server.</returns>
    Task<string> GetSchemaJsonAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the absolute URL the client is currently configured to call.</summary>
    Uri SchemaUri { get; }
}
