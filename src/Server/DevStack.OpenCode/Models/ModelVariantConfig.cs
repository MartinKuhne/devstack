namespace DevStack.OpenCode.Models;

/// <summary>Variant-specific configuration for a model.</summary>
public sealed record ModelVariantConfig
{
    /// <summary>Disable this variant for the model.</summary>
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; init; }
}
