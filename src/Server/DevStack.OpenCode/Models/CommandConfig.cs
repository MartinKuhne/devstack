namespace DevStack.OpenCode.Models;

/// <summary>Custom command configuration.</summary>
public sealed record CommandConfig
{
    /// <summary>Command prompt template.</summary>
    [JsonPropertyName("template")]
    public required string Template { get; init; }

    /// <summary>Human-readable description of the command.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Agent to use when running this command.</summary>
    [JsonPropertyName("agent")]
    public string? Agent { get; init; }

    /// <summary>Model override in the format <c>provider/model</c>.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    /// <summary>Model variant override.</summary>
    [JsonPropertyName("variant")]
    public string? Variant { get; init; }

    /// <summary>Whether to run as a subtask.</summary>
    [JsonPropertyName("subtask")]
    public bool? Subtask { get; init; }
}
