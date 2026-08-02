namespace DevStack.OpenCode.Models;

/// <summary>Represents an OpenCode session returned by the SDK session API.</summary>
public sealed record Session
{
    /// <summary>Unique session identifier.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Owning project identifier.</summary>
    [JsonPropertyName("projectID")]
    public string ProjectId { get; init; } = string.Empty;

    /// <summary>Working directory for the session.</summary>
    [JsonPropertyName("directory")]
    public string Directory { get; init; } = string.Empty;

    /// <summary>Parent session id when this session was forked.</summary>
    [JsonPropertyName("parentID")]
    public string? ParentId { get; init; }

    /// <summary>Optional human-readable title.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>Schema version this session was created against.</summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    /// <summary>Summary block describing the diff totals of the session.</summary>
    [JsonPropertyName("summary")]
    public SessionSummary? Summary { get; init; }

    /// <summary>Share metadata when the session has been shared publicly.</summary>
    [JsonPropertyName("share")]
    public SessionShare? Share { get; init; }

    /// <summary>Revert state if the session is currently reverted to a point in history.</summary>
    [JsonPropertyName("revert")]
    public SessionRevert? Revert { get; init; }

    /// <summary>Session timing metadata.</summary>
    [JsonPropertyName("time")]
    public SessionTime Time { get; init; } = new();

    /// <summary>Additional session properties preserved on the wire.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; init; }
}

/// <summary>Summary totals for a session.</summary>
public sealed record SessionSummary
{
    /// <summary>Total lines added across the session.</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; init; }

    /// <summary>Total lines deleted across the session.</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; init; }

    /// <summary>Total files touched.</summary>
    [JsonPropertyName("files")]
    public int Files { get; init; }

    /// <summary>Optional list of file diffs.</summary>
    [JsonPropertyName("diffs")]
    public IReadOnlyList<FileDiff>? Diffs { get; init; }
}

/// <summary>Share metadata for a session.</summary>
public sealed record SessionShare
{
    /// <summary>Public URL for the shared session.</summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;
}

/// <summary>Revert metadata for a session.</summary>
public sealed record SessionRevert
{
    /// <summary>Identifier of the message the session was reverted to.</summary>
    [JsonPropertyName("messageID")]
    public string MessageId { get; init; } = string.Empty;

    /// <summary>Optional identifier of the part within the message.</summary>
    [JsonPropertyName("partID")]
    public string? PartId { get; init; }

    /// <summary>Optional snapshot identifier associated with the revert point.</summary>
    [JsonPropertyName("snapshot")]
    public string? Snapshot { get; init; }

    /// <summary>Optional diff payload of the revert point.</summary>
    [JsonPropertyName("diff")]
    public string? Diff { get; init; }
}

/// <summary>Timing metadata for a session.</summary>
public sealed record SessionTime
{
    /// <summary>Epoch milliseconds when the session was created.</summary>
    [JsonPropertyName("created")]
    public long Created { get; init; }

    /// <summary>Epoch milliseconds when the session was last updated.</summary>
    [JsonPropertyName("updated")]
    public long Updated { get; init; }

    /// <summary>Epoch milliseconds of the current compaction run, if any.</summary>
    [JsonPropertyName("compacting")]
    public long? Compacting { get; init; }
}

/// <summary>File diff between two revisions.</summary>
public sealed record FileDiff
{
    /// <summary>Path of the changed file.</summary>
    [JsonPropertyName("file")]
    public string File { get; init; } = string.Empty;

    /// <summary>Content before the change.</summary>
    [JsonPropertyName("before")]
    public string Before { get; init; } = string.Empty;

    /// <summary>Content after the change.</summary>
    [JsonPropertyName("after")]
    public string After { get; init; } = string.Empty;

    /// <summary>Lines added.</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; init; }

    /// <summary>Lines deleted.</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; init; }
}

/// <summary>Todo item managed by the session.</summary>
public sealed record Todo
{
    /// <summary>Brief description of the task.</summary>
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    /// <summary>Status: <c>pending</c>, <c>in_progress</c>, <c>completed</c>, or <c>cancelled</c>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>Priority: <c>high</c>, <c>medium</c>, or <c>low</c>.</summary>
    [JsonPropertyName("priority")]
    public string Priority { get; init; } = string.Empty;

    /// <summary>Unique identifier for the todo item.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
}

/// <summary>Status of a single session.</summary>
public sealed record SessionStatusInfo
{
    /// <summary>Discriminator describing the status shape.</summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>Attempt counter for retry statuses.</summary>
    [JsonPropertyName("attempt")]
    public int? Attempt { get; init; }

    /// <summary>Human-readable message for retry statuses.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>Epoch milliseconds of the next scheduled retry.</summary>
    [JsonPropertyName("next")]
    public long? Next { get; init; }
}

/// <summary>Permission request surfaced to the user during a session.</summary>
public sealed record Permission
{
    /// <summary>Unique permission identifier.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Permission type (e.g. <c>bash</c>, <c>edit</c>).</summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>Pattern(s) the permission applies to.</summary>
    [JsonPropertyName("pattern")]
    public PermissionPattern? Pattern { get; init; }

    /// <summary>Owning session id.</summary>
    [JsonPropertyName("sessionID")]
    public string SessionId { get; init; } = string.Empty;

    /// <summary>Owning message id.</summary>
    [JsonPropertyName("messageID")]
    public string MessageId { get; init; } = string.Empty;

    /// <summary>Optional call id of the underlying tool call.</summary>
    [JsonPropertyName("callID")]
    public string? CallId { get; init; }

    /// <summary>Title shown to the user.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>Additional metadata about the request.</summary>
    [JsonPropertyName("metadata")]
    public IDictionary<string, JsonElement>? Metadata { get; init; }

    /// <summary>When the request was raised.</summary>
    [JsonPropertyName("time")]
    public PermissionTime Time { get; init; } = new();
}

/// <summary>Permission pattern — a single value or a list of patterns.</summary>
[JsonConverter(typeof(PermissionPatternConverter))]
public readonly record struct PermissionPattern
{
    private readonly object? _value;

    private PermissionPattern(object? value) => _value = value;

    /// <summary>True when this pattern wraps a single string.</summary>
    public bool IsSingle => _value is string;

    /// <summary>True when this pattern wraps a list of strings.</summary>
    public bool IsMany => _value is IReadOnlyList<string>;

    /// <summary>Single-pattern form, when applicable.</summary>
    public string? Single => _value as string;

    /// <summary>Multi-pattern form, when applicable.</summary>
    public IReadOnlyList<string>? Many => _value as IReadOnlyList<string>;

    /// <summary>Builds a single-pattern form.</summary>
    public static PermissionPattern FromSingle(string value) => new(value);

    /// <summary>Builds a multi-pattern form.</summary>
    public static PermissionPattern FromMany(IReadOnlyList<string> values) => new(values);
}

/// <summary>Timing metadata for a permission request.</summary>
public sealed record PermissionTime
{
    /// <summary>Epoch milliseconds when the request was created.</summary>
    [JsonPropertyName("created")]
    public long Created { get; init; }
}

/// <summary>Allowed response to a permission request.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PermissionResponse>))]
public enum PermissionResponse
{
    /// <summary>Allow once.</summary>
    [JsonStringEnumMemberName("once")] Once,

    /// <summary>Allow and remember.</summary>
    [JsonStringEnumMemberName("always")] Always,

    /// <summary>Reject the request.</summary>
    [JsonStringEnumMemberName("reject")] Reject,
}
