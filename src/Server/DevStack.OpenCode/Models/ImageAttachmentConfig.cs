namespace DevStack.OpenCode.Models;

/// <summary>Image attachment configuration.</summary>
public sealed record ImageAttachmentConfig
{
    /// <summary>Resize images before sending them to the model when they exceed configured limits.</summary>
    [JsonPropertyName("auto_resize")]
    public bool? AutoResize { get; init; }

    /// <summary>Maximum image width before resizing or rejecting the attachment. Default 2000.</summary>
    [JsonPropertyName("max_width")]
    public int? MaxWidth { get; init; }

    /// <summary>Maximum image height before resizing or rejecting the attachment. Default 2000.</summary>
    [JsonPropertyName("max_height")]
    public int? MaxHeight { get; init; }

    /// <summary>Maximum base64 payload bytes for an image attachment. Default 5242880 (5 MiB).</summary>
    [JsonPropertyName("max_base64_bytes")]
    public int? MaxBase64Bytes { get; init; }
}
