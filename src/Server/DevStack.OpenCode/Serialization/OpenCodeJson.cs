using System.Text.Json;

using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Serialization;

/// <summary>
/// Centralized <see cref="JsonSerializerOptions"/> factory for OpenCode SDK
/// types. All read/write paths in the SDK funnel through
/// <see cref="Defaults"/> so that converters and naming policies stay
/// consistent.
/// </summary>
public static class OpenCodeJson
{
    private const int DefaultBufferSize = 16 * 1024;

    /// <summary>Default options: camelCase, indented, null-skipping, custom converters enabled.</summary>
    public static JsonSerializerOptions Defaults { get; } = BuildOptions(writeIndented: true);

    /// <summary>Compact (non-indented) options for network transport.</summary>
    public static JsonSerializerOptions Compact { get; } = BuildOptions(writeIndented: false);

    /// <summary>Builds a new options instance with the given indentation.</summary>
    public static JsonSerializerOptions BuildOptions(bool writeIndented)
    {
        // The OpenCode schema uses mixed snake_case and camelCase (e.g.
        // "disabled_providers" and "logLevel"). We therefore use no naming
        // policy and rely on the explicit [JsonPropertyName] attributes on
        // every model property.
        var options = new JsonSerializerOptions
        {
            WriteIndented = writeIndented,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            DefaultBufferSize = DefaultBufferSize,
        };

        options.Converters.Add(new JsonStringEnumConverter(null));
        options.Converters.Add(new TimeoutValueConverter());
        options.Converters.Add(new AutoUpdateConfigConverter());
        options.Converters.Add(new PermissionConfigConverter());
        options.Converters.Add(new PermissionActionRuleConverter());
        options.Converters.Add(new McpServerConfigConverter());
        options.Converters.Add(new McpOAuthOrDisabledConverter());
        options.Converters.Add(new ReferenceConfigConverter());
        options.Converters.Add(new FormatterConfigConverter());
        options.Converters.Add(new LspConfigConverter());
        options.Converters.Add(new PluginConfigConverter());
        return options;
    }
}
