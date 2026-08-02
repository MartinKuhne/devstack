namespace DevStack.OpenCode.Models;

/// <summary>Discriminated union of all server events.</summary>
[JsonConverter(typeof(EventConverter))]
public sealed record SdkEvent
{
    internal SdkEvent(string type, JsonElement raw)
    {
        Type = type;
        Raw = raw;
    }

    /// <summary>The discriminator derived from the JSON <c>type</c> field.</summary>
    public string Type { get; }

    /// <summary>Raw JSON element backing this event.</summary>
    public JsonElement Raw { get; }

    /// <summary>Returns the <c>properties</c> block of the event, when present.</summary>
    public JsonElement? Properties => Raw.TryGetProperty("properties", out var v) ? v : null;
}

/// <summary>Event envelope that wraps an <see cref="SdkEvent"/> with the originating directory.</summary>
public sealed record GlobalEvent
{
    /// <summary>Owning directory for the event.</summary>
    [JsonPropertyName("directory")]
    public string Directory { get; init; } = string.Empty;

    /// <summary>The wrapped event payload.</summary>
    [JsonPropertyName("payload")]
    public SdkEvent Payload { get; init; } = new("unknown", default);
}
