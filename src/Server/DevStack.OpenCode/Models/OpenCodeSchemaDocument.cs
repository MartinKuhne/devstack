namespace DevStack.OpenCode.Models;

/// <summary>
/// Raw JSON schema document returned by <c>https://opencode.ai/config.json</c>.
/// Use the deserialized model to inspect the schema without committing to the
/// strongly-typed user config surface, or call
/// <see cref="Client.IOpenCodeClient.GetSchemaJsonAsync"/> to retrieve the
/// raw JSON text for validation against user-supplied configs.
/// </summary>
public sealed record OpenCodeSchemaDocument
{
    /// <summary>The <c>$schema</c> URI of the document.</summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }

    /// <summary>The <c>$ref</c> of the root config object.</summary>
    [JsonPropertyName("$ref")]
    public string? Ref { get; init; }

    /// <summary>The named type definitions of the schema.</summary>
    [JsonPropertyName("$defs")]
    public IDictionary<string, JsonElement>? Definitions { get; init; }

    /// <summary>Whether the schema allows JSON comments.</summary>
    [JsonPropertyName("allowComments")]
    public bool? AllowComments { get; init; }

    /// <summary>Whether the schema allows trailing commas.</summary>
    [JsonPropertyName("allowTrailingCommas")]
    public bool? AllowTrailingCommas { get; init; }

    /// <summary>Additional top-level properties preserved verbatim.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; init; }
}
