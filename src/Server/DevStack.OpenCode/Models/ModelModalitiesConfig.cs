namespace DevStack.OpenCode.Models;

/// <summary>Modalities supported by a model.</summary>
public sealed record ModelModalitiesConfig
{
    /// <summary>Accepted input modalities.</summary>
    [JsonPropertyName("input")]
    public IReadOnlyList<Modality>? Input { get; init; }

    /// <summary>Output modalities emitted by the model.</summary>
    [JsonPropertyName("output")]
    public IReadOnlyList<Modality>? Output { get; init; }
}

/// <summary>Modality token (input or output).</summary>
public enum Modality
{
    [JsonStringEnumMemberName("text")] Text,
    [JsonStringEnumMemberName("audio")] Audio,
    [JsonStringEnumMemberName("image")] Image,
    [JsonStringEnumMemberName("video")] Video,
    [JsonStringEnumMemberName("pdf")] Pdf,
}
