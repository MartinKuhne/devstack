using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Store;

/// <summary>
/// Reads and writes <see cref="OpenCodeConfig"/> documents to disk.
/// </summary>
public interface IOpenCodeConfigStore
{
    /// <summary>
    /// Loads the config from <paramref name="path"/>. When the file is
    /// missing, returns an empty <see cref="OpenCodeConfig"/>.
    /// </summary>
    /// <param name="path">Absolute path to the config file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded or empty config.</returns>
    Task<OpenCodeConfig> LoadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the config from the default location. If no explicit
    /// <c>DefaultConfigPath</c> is configured, the store searches
    /// <c>./opencode.json</c> followed by the user-scoped
    /// <c>~/.config/opencode/opencode.json</c>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded or empty config and the path it was loaded from.</returns>
    Task<OpenCodeConfigLoadResult> LoadDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>Serializes and writes the config to <paramref name="path"/>.</summary>
    /// <param name="path">Absolute path to the config file.</param>
    /// <param name="config">Config to write.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveAsync(string path, OpenCodeConfig config, CancellationToken cancellationToken = default);
}

/// <summary>Result of <see cref="IOpenCodeConfigStore.LoadDefaultAsync"/>.</summary>
public sealed record OpenCodeConfigLoadResult(OpenCodeConfig Config, string Path);
