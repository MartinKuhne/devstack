namespace DevStack.OpenCode.Models;

/// <summary>Attachment processing configuration.</summary>
public sealed record AttachmentConfig
{
    /// <summary>Image attachment configuration.</summary>
    [JsonPropertyName("image")]
    public ImageAttachmentConfig? Image { get; init; }
}
