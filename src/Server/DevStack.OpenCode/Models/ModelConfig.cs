namespace DevStack.OpenCode.Models;

/// <summary>Per-model configuration overrides.</summary>
public sealed record ModelConfig
{
    /// <summary>Stable model identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Human-readable model name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Model family grouping.</summary>
    [JsonPropertyName("family")]
    public string? Family { get; init; }

    /// <summary>Model release date (ISO-8601).</summary>
    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; init; }

    /// <summary>Whether the model accepts attachments.</summary>
    [JsonPropertyName("attachment")]
    public bool? Attachment { get; init; }

    /// <summary>Whether the model emits a reasoning trace.</summary>
    [JsonPropertyName("reasoning")]
    public bool? Reasoning { get; init; }

    /// <summary>Whether the temperature parameter is supported.</summary>
    [JsonPropertyName("temperature")]
    public bool? Temperature { get; init; }

    /// <summary>Whether the model supports native tool calls.</summary>
    [JsonPropertyName("tool_call")]
    public bool? ToolCall { get; init; }

    /// <summary>Interleaved thinking configuration.</summary>
    [JsonPropertyName("interleaved")]
    public ModelInterleaved? Interleaved { get; init; }

    /// <summary>Cost in USD per million tokens.</summary>
    [JsonPropertyName("cost")]
    public ModelCostConfig? Cost { get; init; }

    /// <summary>Token limit configuration.</summary>
    [JsonPropertyName("limit")]
    public ModelLimitConfig? Limit { get; init; }

    /// <summary>Supported input/output modalities.</summary>
    [JsonPropertyName("modalities")]
    public ModelModalitiesConfig? Modalities { get; init; }

    /// <summary>Whether the model is experimental.</summary>
    [JsonPropertyName("experimental")]
    public bool? Experimental { get; init; }

    /// <summary>Lifecycle status of the model.</summary>
    [JsonPropertyName("status")]
    public ModelStatus? Status { get; init; }

    /// <summary>Provider hint for nested providers.</summary>
    [JsonPropertyName("provider")]
    public ModelProviderConfig? Provider { get; init; }

    /// <summary>Provider-specific options.</summary>
    [JsonPropertyName("options")]
    public IDictionary<string, JsonElement>? Options { get; init; }

    /// <summary>Additional HTTP headers to send with requests to this model.</summary>
    [JsonPropertyName("headers")]
    public IDictionary<string, string>? Headers { get; init; }

    /// <summary>Variant-specific configuration.</summary>
    [JsonPropertyName("variants")]
    public IDictionary<string, ModelVariantConfig>? Variants { get; init; }
}
