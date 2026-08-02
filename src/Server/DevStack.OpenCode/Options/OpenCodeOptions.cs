namespace DevStack.OpenCode.Options;

/// <summary>
/// Configuration for the OpenCode SDK. Bound from the <c>OpenCode</c>
/// configuration section.
/// </summary>
public sealed class OpenCodeOptions
{
    /// <summary>Configuration section name to bind from <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.</summary>
    public const string SectionName = "OpenCode";

    /// <summary>
    /// Base URL of the OpenCode schema host. Defaults to <c>https://opencode.ai/</c>.
    /// </summary>
    public Uri BaseUrl { get; set; } = new Uri("https://opencode.ai/");

    /// <summary>
    /// Path to the canonical schema endpoint, relative to <see cref="BaseUrl"/>.
    /// Defaults to <c>config.json</c>.
    /// </summary>
    public string SchemaPath { get; set; } = "config.json";

    /// <summary>
    /// Default file path used by <see cref="DevStack.OpenCode.Store.IOpenCodeConfigStore"/> when no
    /// explicit path is supplied. When <c>null</c>, the store searches
    /// <c>./opencode.json</c> and <c>~/.config/opencode/opencode.json</c>.
    /// </summary>
    public string? DefaultConfigPath { get; set; }

    /// <summary>
    /// HTTP request timeout for fetching the schema. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>User-Agent header to send with schema requests.</summary>
    public string UserAgent { get; set; } = "DevStack.OpenCode/1.0";

    /// <summary>
    /// Resolves the absolute URI of the schema endpoint by combining
    /// <see cref="BaseUrl"/> with <see cref="SchemaPath"/>. Ensures the base
    /// has a trailing slash so the relative path is appended rather than
    /// replacing the last segment.
    /// </summary>
    public Uri ResolveSchemaUri()
    {
        var baseUri = BaseUrl;
        var baseString = baseUri.ToString();
        if (!baseString.EndsWith('/'))
        {
            baseUri = new Uri(baseString + "/", UriKind.Absolute);
        }

        return new Uri(baseUri, SchemaPath);
    }
}
