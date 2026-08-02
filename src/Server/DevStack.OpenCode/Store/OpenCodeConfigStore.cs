using DevStack.OpenCode.Models;
using DevStack.OpenCode.Options;
using DevStack.OpenCode.Serialization;

using Microsoft.Extensions.Options;

namespace DevStack.OpenCode.Store;

/// <summary>
/// Default <see cref="IOpenCodeConfigStore"/> implementation. Reads and
/// writes JSON files from the local file system using the centralized
/// <see cref="OpenCodeJson"/> serializer options.
/// </summary>
public sealed class OpenCodeConfigStore : IOpenCodeConfigStore
{
    /// <summary>Default filename searched in the working directory.</summary>
    public const string DefaultFileName = "opencode.json";

    private const string UserConfigDirectory = ".config/opencode";
    private const string UserConfigFileName = "opencode.json";

    private readonly IOptions<OpenCodeOptions> _options;
    private readonly ILogger<OpenCodeConfigStore> _logger;

    /// <summary>
    /// Creates a new <see cref="OpenCodeConfigStore"/>.
    /// </summary>
    /// <param name="options">SDK options. Used for the default file path.</param>
    /// <param name="logger">Optional logger.</param>
    public OpenCodeConfigStore(
        IOptions<OpenCodeOptions> options,
        ILogger<OpenCodeConfigStore>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<OpenCodeConfigStore>.Instance;
    }

    /// <inheritdoc />
    public async Task<OpenCodeConfig> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must be a non-empty absolute path.", nameof(path));
        }

        if (!File.Exists(path))
        {
            _logger.LogDebug("OpenCode config file {Path} does not exist; returning empty config.", path);
            return new OpenCodeConfig();
        }

        await using var stream = File.OpenRead(path);
        var config = await JsonSerializer
            .DeserializeAsync<OpenCodeConfig>(stream, OpenCodeJson.Compact, cancellationToken)
            .ConfigureAwait(false);

        if (config is null)
        {
            _logger.LogWarning("OpenCode config file {Path} deserialized to null; returning empty config.", path);
            return new OpenCodeConfig();
        }

        return config;
    }

    /// <inheritdoc />
    public async Task<OpenCodeConfigLoadResult> LoadDefaultAsync(CancellationToken cancellationToken = default)
    {
        var path = ResolveDefaultPath();
        var config = await LoadAsync(path, cancellationToken).ConfigureAwait(false);
        return new OpenCodeConfigLoadResult(config, path);
    }

    /// <inheritdoc />
    public async Task SaveAsync(string path, OpenCodeConfig config, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must be a non-empty absolute path.", nameof(path));
        }

        ArgumentNullException.ThrowIfNull(config);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(path);
        await JsonSerializer
            .SerializeAsync(stream, config, OpenCodeJson.Defaults, cancellationToken)
            .ConfigureAwait(false);
    }

    private string ResolveDefaultPath()
    {
        var explicitPath = _options.Value.DefaultConfigPath;
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            return Path.GetFullPath(explicitPath);
        }

        var workingDir = Path.Combine(Directory.GetCurrentDirectory(), DefaultFileName);
        if (File.Exists(workingDir))
        {
            return workingDir;
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(home))
        {
            var userPath = Path.Combine(home, UserConfigDirectory, UserConfigFileName);
            if (File.Exists(userPath))
            {
                return userPath;
            }
        }

        // No existing file; return the working-directory path as the default
        // for round-tripping SaveAsync.
        return workingDir;
    }
}
