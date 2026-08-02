namespace DevStack.OpenCode.Models;

/// <summary>Represents an LLM provider exposed by the OpenCode server.</summary>
public sealed record Provider
{
    /// <summary>Stable provider identifier.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;

    /// <summary>Human-readable provider name.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    /// <summary>Source of the provider configuration: <c>env</c>, <c>config</c>, <c>custom</c>, or <c>api</c>.</summary>
    [JsonPropertyName("source")] public string Source { get; init; } = string.Empty;

    /// <summary>Environment variable names consulted for credentials.</summary>
    [JsonPropertyName("env")] public IReadOnlyList<string> Env { get; init; } = Array.Empty<string>();

    /// <summary>Optional API key (only present in some server responses).</summary>
    [JsonPropertyName("key")] public string? Key { get; init; }

    /// <summary>Provider-level options.</summary>
    [JsonPropertyName("options")] public IDictionary<string, JsonElement>? Options { get; init; }

    /// <summary>Models exposed by this provider.</summary>
    [JsonPropertyName("models")] public IDictionary<string, Model> Models { get; init; } = new Dictionary<string, Model>();
}

/// <summary>Represents a single model on a provider.</summary>
public sealed record Model
{
    /// <summary>Model identifier scoped to the provider.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;

    /// <summary>Owning provider identifier.</summary>
    [JsonPropertyName("providerID")] public string ProviderId { get; init; } = string.Empty;

    /// <summary>API descriptor for the model.</summary>
    [JsonPropertyName("api")] public ModelApi Api { get; init; } = new();

    /// <summary>Human-readable model name.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    /// <summary>Capability flags for the model.</summary>
    [JsonPropertyName("capabilities")] public ModelCapabilities Capabilities { get; init; } = new();

    /// <summary>Cost in USD per million tokens.</summary>
    [JsonPropertyName("cost")] public ModelCost Cost { get; init; } = new();

    /// <summary>Token limits.</summary>
    [JsonPropertyName("limit")] public ModelLimits Limit { get; init; } = new();

    /// <summary>Lifecycle status.</summary>
    [JsonPropertyName("status")] public string Status { get; init; } = "active";

    /// <summary>Provider-specific request options.</summary>
    [JsonPropertyName("options")] public IDictionary<string, JsonElement> Options { get; init; } = new Dictionary<string, JsonElement>();

    /// <summary>Additional HTTP headers.</summary>
    [JsonPropertyName("headers")] public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}

/// <summary>API descriptor for a model.</summary>
public sealed record ModelApi
{
    /// <summary>Provider SDK API identifier.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>API base URL.</summary>
    [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
    /// <summary>NPM package implementing the API.</summary>
    [JsonPropertyName("npm")] public string Npm { get; init; } = string.Empty;
}

/// <summary>Capability flags for a model.</summary>
public sealed record ModelCapabilities
{
    /// <summary>Whether the model supports the <c>temperature</c> parameter.</summary>
    [JsonPropertyName("temperature")] public bool Temperature { get; init; }
    /// <summary>Whether the model emits reasoning traces.</summary>
    [JsonPropertyName("reasoning")] public bool Reasoning { get; init; }
    /// <summary>Whether the model accepts attachments.</summary>
    [JsonPropertyName("attachment")] public bool Attachment { get; init; }
    /// <summary>Whether the model supports native tool calling.</summary>
    [JsonPropertyName("toolcall")] public bool Toolcall { get; init; }
    /// <summary>Supported input modalities.</summary>
    [JsonPropertyName("input")] public ModelModalitySet Input { get; init; } = new();
    /// <summary>Supported output modalities.</summary>
    [JsonPropertyName("output")] public ModelModalitySet Output { get; init; } = new();
}

/// <summary>Modality support flags.</summary>
public sealed record ModelModalitySet
{
    /// <summary>Text modality.</summary>
    [JsonPropertyName("text")] public bool Text { get; init; }
    /// <summary>Audio modality.</summary>
    [JsonPropertyName("audio")] public bool Audio { get; init; }
    /// <summary>Image modality.</summary>
    [JsonPropertyName("image")] public bool Image { get; init; }
    /// <summary>Video modality.</summary>
    [JsonPropertyName("video")] public bool Video { get; init; }
    /// <summary>PDF modality.</summary>
    [JsonPropertyName("pdf")] public bool Pdf { get; init; }
}

/// <summary>Cost in USD per million tokens.</summary>
public sealed record ModelCost
{
    /// <summary>Input token cost.</summary>
    [JsonPropertyName("input")] public double Input { get; init; }
    /// <summary>Output token cost.</summary>
    [JsonPropertyName("output")] public double Output { get; init; }
    /// <summary>Cache read/write costs.</summary>
    [JsonPropertyName("cache")] public ModelCacheCost Cache { get; init; } = new();
    /// <summary>Experimental over-200k context cost.</summary>
    [JsonPropertyName("experimentalOver200K")] public ModelCacheCost? ExperimentalOver200K { get; init; }
}

/// <summary>Cache read/write costs in USD per million tokens.</summary>
public sealed record ModelCacheCost
{
    /// <summary>Cache read cost.</summary>
    [JsonPropertyName("read")] public double Read { get; init; }
    /// <summary>Cache write cost.</summary>
    [JsonPropertyName("write")] public double Write { get; init; }
}

/// <summary>Token limits.</summary>
public sealed record ModelLimits
{
    /// <summary>Total context window size in tokens.</summary>
    [JsonPropertyName("context")] public int Context { get; init; }
    /// <summary>Maximum output tokens.</summary>
    [JsonPropertyName("output")] public int Output { get; init; }
}

/// <summary>Provider authentication method descriptor.</summary>
public sealed record ProviderAuthMethod
{
    /// <summary>Either <c>oauth</c> or <c>api</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = string.Empty;
    /// <summary>Human-readable label for the method.</summary>
    [JsonPropertyName("label")] public string Label { get; init; } = string.Empty;
}

/// <summary>Result of provider OAuth authorization.</summary>
public sealed record ProviderAuthAuthorization
{
    /// <summary>URL the user should visit to complete authorization.</summary>
    [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
    /// <summary>Authorization method — <c>auto</c> or <c>code</c>.</summary>
    [JsonPropertyName("method")] public string Method { get; init; } = "auto";
    /// <summary>User-facing instructions for completing the flow.</summary>
    [JsonPropertyName("instructions")] public string Instructions { get; init; } = string.Empty;
}

/// <summary>Response from <c>GET /provider</c>.</summary>
public sealed record ProviderListResponse
{
    /// <summary>All available providers.</summary>
    [JsonPropertyName("all")] public IReadOnlyList<Provider> All { get; init; } = Array.Empty<Provider>();
    /// <summary>Default model per provider.</summary>
    [JsonPropertyName("default")] public IDictionary<string, string> Default { get; init; } = new Dictionary<string, string>();
    /// <summary>Provider ids that are currently connected.</summary>
    [JsonPropertyName("connected")] public IReadOnlyList<string> Connected { get; init; } = Array.Empty<string>();
}
