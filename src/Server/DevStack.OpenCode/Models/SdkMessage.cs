namespace DevStack.OpenCode.Models;

/// <summary>Discriminated union of message types. Read with the typed accessors.</summary>
[JsonConverter(typeof(MessageConverter))]
public sealed record Message
{
    internal Message(string kind, JsonElement raw)
    {
        Kind = kind;
        Raw = raw;
    }

    /// <summary>The discriminator derived from the JSON <c>role</c> field.</summary>
    public string Kind { get; }

    /// <summary>Raw JSON element backing this message.</summary>
    public JsonElement Raw { get; }

    /// <summary>True when the message is a <see cref="UserMessage"/>.</summary>
    public bool IsUser => Kind == "user";

    /// <summary>True when the message is an <see cref="AssistantMessage"/>.</summary>
    public bool IsAssistant => Kind == "assistant";

    /// <summary>Returns the message as a <see cref="UserMessage"/>.</summary>
    public UserMessage AsUser() => JsonSerializer.Deserialize<UserMessage>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize user message.");

    /// <summary>Returns the message as an <see cref="AssistantMessage"/>.</summary>
    public AssistantMessage AsAssistant() => JsonSerializer.Deserialize<AssistantMessage>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize assistant message.");
}

/// <summary>Message produced by the user.</summary>
public sealed record UserMessage
{
    /// <summary>Unique message identifier.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")]
    public string SessionId { get; init; } = string.Empty;

    /// <summary>Always <c>user</c>.</summary>
    [JsonPropertyName("role")]
    public string Role { get; init; } = "user";

    /// <summary>Timing metadata.</summary>
    [JsonPropertyName("time")]
    public MessageTime Time { get; init; } = new();

    /// <summary>Optional summary of the user's intent.</summary>
    [JsonPropertyName("summary")]
    public UserMessageSummary? Summary { get; init; }

    /// <summary>Agent invoked for this message.</summary>
    [JsonPropertyName("agent")]
    public string Agent { get; init; } = string.Empty;

    /// <summary>Model used for the agent.</summary>
    [JsonPropertyName("model")]
    public ModelRef Model { get; init; } = new();

    /// <summary>Optional system prompt override.</summary>
    [JsonPropertyName("system")]
    public string? System { get; init; }

    /// <summary>Optional per-tool enable/disable override.</summary>
    [JsonPropertyName("tools")]
    public IDictionary<string, bool>? Tools { get; init; }
}

/// <summary>Message produced by the assistant.</summary>
public sealed record AssistantMessage
{
    /// <summary>Unique message identifier.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")]
    public string SessionId { get; init; } = string.Empty;

    /// <summary>Always <c>assistant</c>.</summary>
    [JsonPropertyName("role")]
    public string Role { get; init; } = "assistant";

    /// <summary>Timing metadata.</summary>
    [JsonPropertyName("time")]
    public AssistantMessageTime Time { get; init; } = new();

    /// <summary>Optional error payload.</summary>
    [JsonPropertyName("error")]
    public MessageError? Error { get; init; }

    /// <summary>Id of the parent message.</summary>
    [JsonPropertyName("parentID")]
    public string ParentId { get; init; } = string.Empty;

    /// <summary>Model identifier (without provider prefix).</summary>
    [JsonPropertyName("modelID")]
    public string ModelId { get; init; } = string.Empty;

    /// <summary>Provider identifier.</summary>
    [JsonPropertyName("providerID")]
    public string ProviderId { get; init; } = string.Empty;

    /// <summary>Mode used for the assistant run.</summary>
    [JsonPropertyName("mode")]
    public string Mode { get; init; } = string.Empty;

    /// <summary>Path metadata for the assistant run.</summary>
    [JsonPropertyName("path")]
    public MessagePath Path { get; init; } = new();

    /// <summary>True when this message is a summary produced by compaction.</summary>
    [JsonPropertyName("summary")]
    public bool? Summary { get; init; }

    /// <summary>Cost incurred by this message in USD.</summary>
    [JsonPropertyName("cost")]
    public double Cost { get; init; }

    /// <summary>Token usage for this message.</summary>
    [JsonPropertyName("tokens")]
    public TokenUsage Tokens { get; init; } = new();

    /// <summary>Finish reason reported by the model.</summary>
    [JsonPropertyName("finish")]
    public string? Finish { get; init; }
}

/// <summary>User message summary block.</summary>
public sealed record UserMessageSummary
{
    /// <summary>Optional title of the user message summary.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>Optional body text.</summary>
    [JsonPropertyName("body")]
    public string? Body { get; init; }

    /// <summary>File diffs associated with the summary.</summary>
    [JsonPropertyName("diffs")]
    public IReadOnlyList<FileDiff> Diffs { get; init; } = Array.Empty<FileDiff>();
}

/// <summary>Reference to a provider/model pair.</summary>
public sealed record ModelRef
{
    /// <summary>Provider identifier.</summary>
    [JsonPropertyName("providerID")]
    public string ProviderId { get; init; } = string.Empty;

    /// <summary>Model identifier.</summary>
    [JsonPropertyName("modelID")]
    public string ModelId { get; init; } = string.Empty;
}

/// <summary>Timing metadata for a user message.</summary>
public sealed record MessageTime
{
    /// <summary>Epoch milliseconds when the message was created.</summary>
    [JsonPropertyName("created")]
    public long Created { get; init; }
}

/// <summary>Timing metadata for an assistant message.</summary>
public sealed record AssistantMessageTime
{
    /// <summary>Epoch milliseconds when the message was created.</summary>
    [JsonPropertyName("created")]
    public long Created { get; init; }

    /// <summary>Epoch milliseconds when the message completed.</summary>
    [JsonPropertyName("completed")]
    public long? Completed { get; init; }
}

/// <summary>Path metadata for an assistant message.</summary>
public sealed record MessagePath
{
    /// <summary>Current working directory at the time of the run.</summary>
    [JsonPropertyName("cwd")]
    public string Cwd { get; init; } = string.Empty;

    /// <summary>Project root at the time of the run.</summary>
    [JsonPropertyName("root")]
    public string Root { get; init; } = string.Empty;
}

/// <summary>Token usage for an assistant message.</summary>
public sealed record TokenUsage
{
    /// <summary>Input tokens consumed.</summary>
    [JsonPropertyName("input")]
    public int Input { get; init; }

    /// <summary>Output tokens produced.</summary>
    [JsonPropertyName("output")]
    public int Output { get; init; }

    /// <summary>Reasoning tokens produced.</summary>
    [JsonPropertyName("reasoning")]
    public int Reasoning { get; init; }

    /// <summary>Cache read/write tokens.</summary>
    [JsonPropertyName("cache")]
    public CacheTokens Cache { get; init; } = new();
}

/// <summary>Cache token usage.</summary>
public sealed record CacheTokens
{
    /// <summary>Tokens read from the prompt cache.</summary>
    [JsonPropertyName("read")]
    public int Read { get; init; }

    /// <summary>Tokens written to the prompt cache.</summary>
    [JsonPropertyName("write")]
    public int Write { get; init; }
}

/// <summary>Discriminated union of error payloads on assistant messages.</summary>
[JsonConverter(typeof(MessageErrorConverter))]
public sealed record MessageError
{
    internal MessageError(string kind, JsonElement raw)
    {
        Kind = kind;
        Raw = raw;
    }

    /// <summary>The discriminator derived from the JSON <c>name</c> field.</summary>
    public string Kind { get; }

    /// <summary>Raw JSON element backing this error.</summary>
    public JsonElement Raw { get; }

    /// <summary>Returns the error message string from the underlying payload.</summary>
    public string? Message => Raw.TryGetProperty("data", out var data) && data.TryGetProperty("message", out var msg)
        ? msg.GetString()
        : null;
}
