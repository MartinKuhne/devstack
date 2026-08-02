namespace DevStack.OpenCode.Models;

/// <summary>Skill folder configuration.</summary>
public sealed record SkillsConfig
{
    /// <summary>Additional paths to skill folders.</summary>
    [JsonPropertyName("paths")]
    public IReadOnlyList<string>? Paths { get; init; }

    /// <summary>URLs to fetch skills from (e.g. <c>https://example.com/.well-known/skills/</c>).</summary>
    [JsonPropertyName("urls")]
    public IReadOnlyList<string>? Urls { get; init; }
}
