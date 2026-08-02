namespace DevStack.OpenCode.Models;

/// <summary>
/// A single OpenCode plugin entry. May be either a bare name (e.g. an NPM
/// package string) or a two-element tuple of <c>[name, options]</c>.
/// </summary>
[JsonConverter(typeof(PluginConfigConverter))]
public readonly record struct PluginConfig : IEquatable<PluginConfig>
{
    private PluginConfig(string? name, IDictionary<string, JsonElement>? options)
    {
        Name = name;
        Options = options;
    }

    /// <summary>Plugin name (e.g. NPM package name).</summary>
    public string? Name { get; }

    /// <summary>Plugin options when supplied as a tuple; <c>null</c> for a bare name.</summary>
    public IDictionary<string, JsonElement>? Options { get; }

    /// <summary>True when the plugin entry carries options.</summary>
    public bool HasOptions => Options is { Count: > 0 };

    /// <summary>Builds a bare-name plugin entry.</summary>
    public static PluginConfig FromName(string name) => new(name, null);

    /// <summary>Builds a <c>[name, options]</c> plugin entry.</summary>
    public static PluginConfig FromTuple(string name, IDictionary<string, JsonElement> options) =>
        new(name, options);

    public bool Equals(PluginConfig other) => Name == other.Name && Equals(Options, other.Options);
    public override int GetHashCode() => HashCode.Combine(Name, Options);
    public override string ToString() => HasOptions ? $"{Name} (with options)" : Name ?? string.Empty;
}
