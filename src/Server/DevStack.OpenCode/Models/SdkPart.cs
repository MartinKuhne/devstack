namespace DevStack.OpenCode.Models;

/// <summary>Discriminated union of part types. Read with the typed accessors.</summary>
[JsonConverter(typeof(PartConverter))]
public sealed record Part
{
    internal Part(string kind, JsonElement raw)
    {
        Kind = kind;
        Raw = raw;
    }

    /// <summary>The discriminator derived from the JSON <c>type</c> field.</summary>
    public string Kind { get; }

    /// <summary>Raw JSON element backing this part.</summary>
    public JsonElement Raw { get; }

    /// <summary>Common part id.</summary>
    public string Id => Raw.TryGetProperty("id", out var v) ? v.GetString() ?? string.Empty : string.Empty;

    /// <summary>Owning session id.</summary>
    public string SessionId => Raw.TryGetProperty("sessionID", out var v) ? v.GetString() ?? string.Empty : string.Empty;

    /// <summary>Owning message id.</summary>
    public string MessageId => Raw.TryGetProperty("messageID", out var v) ? v.GetString() ?? string.Empty : string.Empty;

    /// <summary>Deserializes the part as a <see cref="TextPart"/>.</summary>
    public TextPart AsText() => JsonSerializer.Deserialize<TextPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize text part.");

    /// <summary>Deserializes the part as a <see cref="ReasoningPart"/>.</summary>
    public ReasoningPart AsReasoning() => JsonSerializer.Deserialize<ReasoningPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize reasoning part.");

    /// <summary>Deserializes the part as a <see cref="FilePart"/>.</summary>
    public FilePart AsFile() => JsonSerializer.Deserialize<FilePart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize file part.");

    /// <summary>Deserializes the part as a <see cref="ToolPart"/>.</summary>
    public ToolPart AsTool() => JsonSerializer.Deserialize<ToolPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize tool part.");

    /// <summary>Deserializes the part as a <see cref="SubtaskPart"/>.</summary>
    public SubtaskPart AsSubtask() => JsonSerializer.Deserialize<SubtaskPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize subtask part.");

    /// <summary>Deserializes the part as a <see cref="StepStartPart"/>.</summary>
    public StepStartPart AsStepStart() => JsonSerializer.Deserialize<StepStartPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize step-start part.");

    /// <summary>Deserializes the part as a <see cref="StepFinishPart"/>.</summary>
    public StepFinishPart AsStepFinish() => JsonSerializer.Deserialize<StepFinishPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize step-finish part.");

    /// <summary>Deserializes the part as a <see cref="SnapshotPart"/>.</summary>
    public SnapshotPart AsSnapshot() => JsonSerializer.Deserialize<SnapshotPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize snapshot part.");

    /// <summary>Deserializes the part as a <see cref="PatchPart"/>.</summary>
    public PatchPart AsPatch() => JsonSerializer.Deserialize<PatchPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize patch part.");

    /// <summary>Deserializes the part as a <see cref="AgentPart"/>.</summary>
    public AgentPart AsAgent() => JsonSerializer.Deserialize<AgentPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize agent part.");

    /// <summary>Deserializes the part as a <see cref="RetryPart"/>.</summary>
    public RetryPart AsRetry() => JsonSerializer.Deserialize<RetryPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize retry part.");

    /// <summary>Deserializes the part as a <see cref="CompactionPart"/>.</summary>
    public CompactionPart AsCompaction() => JsonSerializer.Deserialize<CompactionPart>(Raw.GetRawText(), OpenCodeJson.Compact)
        ?? throw new InvalidOperationException("Failed to deserialize compaction part.");
}

/// <summary>Plain text part.</summary>
public sealed record TextPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>text</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "text";
    /// <summary>Plain text content.</summary>
    [JsonPropertyName("text")] public string Text { get; init; } = string.Empty;
    /// <summary>True when the part was synthesized rather than user-supplied.</summary>
    [JsonPropertyName("synthetic")] public bool? Synthetic { get; init; }
    /// <summary>True when the part is ignored during processing.</summary>
    [JsonPropertyName("ignored")] public bool? Ignored { get; init; }
    /// <summary>Timing metadata.</summary>
    [JsonPropertyName("time")] public TextPartTime? Time { get; init; }
    /// <summary>Additional metadata preserved on the wire.</summary>
    [JsonPropertyName("metadata")] public IDictionary<string, JsonElement>? Metadata { get; init; }
}

/// <summary>Timing metadata for a text part.</summary>
public sealed record TextPartTime
{
    /// <summary>Epoch milliseconds when the part started rendering.</summary>
    [JsonPropertyName("start")] public long Start { get; init; }
    /// <summary>Epoch milliseconds when the part finished rendering.</summary>
    [JsonPropertyName("end")] public long? End { get; init; }
}

/// <summary>Reasoning part emitted by a reasoning model.</summary>
public sealed record ReasoningPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>reasoning</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "reasoning";
    /// <summary>Reasoning text.</summary>
    [JsonPropertyName("text")] public string Text { get; init; } = string.Empty;
    /// <summary>Timing metadata.</summary>
    [JsonPropertyName("time")] public TextPartTime? Time { get; init; }
    /// <summary>Additional metadata preserved on the wire.</summary>
    [JsonPropertyName("metadata")] public IDictionary<string, JsonElement>? Metadata { get; init; }
}

/// <summary>File attachment part.</summary>
public sealed record FilePart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>file</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "file";
    /// <summary>Mime type of the attachment.</summary>
    [JsonPropertyName("mime")] public string Mime { get; init; } = string.Empty;
    /// <summary>Optional filename.</summary>
    [JsonPropertyName("filename")] public string? Filename { get; init; }
    /// <summary>URL where the file can be fetched.</summary>
    [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
    /// <summary>Optional source descriptor (file path or symbol reference).</summary>
    [JsonPropertyName("source")] public FilePartSource? Source { get; init; }
}

/// <summary>Source descriptor for a file part.</summary>
[JsonConverter(typeof(FilePartSourceConverter))]
public sealed record FilePartSource
{
    internal FilePartSource(string kind, JsonElement raw)
    {
        Kind = kind;
        Raw = raw;
    }

    /// <summary>Discriminator — <c>file</c> or <c>symbol</c>.</summary>
    public string Kind { get; }

    /// <summary>Raw JSON element.</summary>
    public JsonElement Raw { get; }

    /// <summary>File path on disk.</summary>
    public string Path => Raw.TryGetProperty("path", out var v) ? v.GetString() ?? string.Empty : string.Empty;
}

/// <summary>Tool invocation part.</summary>
public sealed record ToolPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>tool</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "tool";
    /// <summary>Tool call id.</summary>
    [JsonPropertyName("callID")] public string CallId { get; init; } = string.Empty;
    /// <summary>Tool name.</summary>
    [JsonPropertyName("tool")] public string Tool { get; init; } = string.Empty;
    /// <summary>Current tool state (pending, running, completed, error).</summary>
    [JsonPropertyName("state")] public ToolState? State { get; init; }
    /// <summary>Additional metadata preserved on the wire.</summary>
    [JsonPropertyName("metadata")] public IDictionary<string, JsonElement>? Metadata { get; init; }
}

/// <summary>Discriminated union of tool states.</summary>
[JsonConverter(typeof(ToolStateConverter))]
public sealed record ToolState
{
    internal ToolState(string status, JsonElement raw)
    {
        Status = status;
        Raw = raw;
    }

    /// <summary>Status discriminator.</summary>
    public string Status { get; }

    /// <summary>Raw JSON element.</summary>
    public JsonElement Raw { get; }
}

/// <summary>Subtask delegation part.</summary>
public sealed record SubtaskPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>subtask</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "subtask";
    /// <summary>Prompt delegated to the sub-agent.</summary>
    [JsonPropertyName("prompt")] public string Prompt { get; init; } = string.Empty;
    /// <summary>Description of the subtask.</summary>
    [JsonPropertyName("description")] public string Description { get; init; } = string.Empty;
    /// <summary>Agent name invoked.</summary>
    [JsonPropertyName("agent")] public string Agent { get; init; } = string.Empty;
}

/// <summary>Step-start marker part.</summary>
public sealed record StepStartPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>step-start</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "step-start";
    /// <summary>Optional snapshot reference.</summary>
    [JsonPropertyName("snapshot")] public string? Snapshot { get; init; }
}

/// <summary>Step-finish marker part.</summary>
public sealed record StepFinishPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>step-finish</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "step-finish";
    /// <summary>Reason for finishing the step.</summary>
    [JsonPropertyName("reason")] public string Reason { get; init; } = string.Empty;
    /// <summary>Optional snapshot reference.</summary>
    [JsonPropertyName("snapshot")] public string? Snapshot { get; init; }
    /// <summary>Cost in USD for this step.</summary>
    [JsonPropertyName("cost")] public double Cost { get; init; }
    /// <summary>Token usage for this step.</summary>
    [JsonPropertyName("tokens")] public TokenUsage Tokens { get; init; } = new();
}

/// <summary>Filesystem snapshot part.</summary>
public sealed record SnapshotPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>snapshot</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "snapshot";
    /// <summary>Snapshot identifier.</summary>
    [JsonPropertyName("snapshot")] public string Snapshot { get; init; } = string.Empty;
}

/// <summary>Patch part capturing a set of file changes.</summary>
public sealed record PatchPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>patch</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "patch";
    /// <summary>Hash identifying the patch.</summary>
    [JsonPropertyName("hash")] public string Hash { get; init; } = string.Empty;
    /// <summary>Files touched by the patch.</summary>
    [JsonPropertyName("files")] public IReadOnlyList<string> Files { get; init; } = Array.Empty<string>();
}

/// <summary>Agent invocation part.</summary>
public sealed record AgentPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>agent</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "agent";
    /// <summary>Name of the invoked agent.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    /// <summary>Optional source span.</summary>
    [JsonPropertyName("source")] public SourceSpan? Source { get; init; }
}

/// <summary>Source span descriptor.</summary>
public sealed record SourceSpan
{
    /// <summary>Span text.</summary>
    [JsonPropertyName("value")] public string Value { get; init; } = string.Empty;
    /// <summary>Start offset in the source.</summary>
    [JsonPropertyName("start")] public int Start { get; init; }
    /// <summary>End offset in the source.</summary>
    [JsonPropertyName("end")] public int End { get; init; }
}

/// <summary>Retry marker part.</summary>
public sealed record RetryPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>retry</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "retry";
    /// <summary>Retry attempt number.</summary>
    [JsonPropertyName("attempt")] public int Attempt { get; init; }
    /// <summary>Error that triggered the retry.</summary>
    [JsonPropertyName("error")] public JsonElement Error { get; init; }
    /// <summary>Timing metadata.</summary>
    [JsonPropertyName("time")] public RetryPartTime Time { get; init; } = new();
}

/// <summary>Timing metadata for a retry part.</summary>
public sealed record RetryPartTime
{
    /// <summary>Epoch milliseconds when the retry was scheduled.</summary>
    [JsonPropertyName("created")] public long Created { get; init; }
}

/// <summary>Compaction marker part.</summary>
public sealed record CompactionPart
{
    /// <summary>Unique part id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")] public string SessionId { get; init; } = string.Empty;
    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Always <c>compaction</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "compaction";
    /// <summary>True when the compaction was triggered automatically.</summary>
    [JsonPropertyName("auto")] public bool Auto { get; init; }
}
