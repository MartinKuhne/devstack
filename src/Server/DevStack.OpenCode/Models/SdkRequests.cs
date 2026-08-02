namespace DevStack.OpenCode.Models;

/// <summary>Body for <c>POST /session</c>.</summary>
public sealed record SessionCreateRequest
{
    /// <summary>Optional parent session id when forking.</summary>
    [JsonPropertyName("parentID")] public string? ParentId { get; init; }
    /// <summary>Optional human-readable title.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }
}

/// <summary>Body for <c>PATCH /session/{id}</c>.</summary>
public sealed record SessionUpdateRequest
{
    /// <summary>New title for the session.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }
}

/// <summary>Body for <c>POST /session/{id}/init</c>.</summary>
public sealed record SessionInitRequest
{
    /// <summary>Model identifier (without provider prefix).</summary>
    [JsonPropertyName("modelID")] public string ModelId { get; init; } = string.Empty;
    /// <summary>Provider identifier.</summary>
    [JsonPropertyName("providerID")] public string ProviderId { get; init; } = string.Empty;
    /// <summary>Anchor message id.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
}

/// <summary>Body for <c>POST /session/{id}/fork</c>.</summary>
public sealed record SessionForkRequest
{
    /// <summary>Optional message id at which to fork.</summary>
    [JsonPropertyName("messageID")] public string? MessageId { get; init; }
}

/// <summary>Body for <c>POST /session/{id}/summarize</c>.</summary>
public sealed record SessionSummarizeRequest
{
    /// <summary>Provider identifier used for the summary.</summary>
    [JsonPropertyName("providerID")] public string ProviderId { get; init; } = string.Empty;
    /// <summary>Model identifier used for the summary.</summary>
    [JsonPropertyName("modelID")] public string ModelId { get; init; } = string.Empty;
}

/// <summary>Body for <c>POST /session/{id}/message</c>.</summary>
public sealed record SessionPromptRequest
{
    /// <summary>Optional message id to continue from.</summary>
    [JsonPropertyName("messageID")] public string? MessageId { get; init; }
    /// <summary>Model override.</summary>
    [JsonPropertyName("model")] public ModelRef? Model { get; init; }
    /// <summary>Agent override.</summary>
    [JsonPropertyName("agent")] public string? Agent { get; init; }
    /// <summary>True when no AI response should be generated (context-only).</summary>
    [JsonPropertyName("noReply")] public bool? NoReply { get; init; }
    /// <summary>Optional system prompt override.</summary>
    [JsonPropertyName("system")] public string? System { get; init; }
    /// <summary>Optional per-tool enable/disable override.</summary>
    [JsonPropertyName("tools")] public IDictionary<string, bool>? Tools { get; init; }
    /// <summary>Parts composing the message.</summary>
    [JsonPropertyName("parts")] public IReadOnlyList<PartInput> Parts { get; init; } = Array.Empty<PartInput>();
}

/// <summary>Body for <c>POST /session/{id}/command</c>.</summary>
public sealed record SessionCommandRequest
{
    /// <summary>Optional message id to continue from.</summary>
    [JsonPropertyName("messageID")] public string? MessageId { get; init; }
    /// <summary>Agent override.</summary>
    [JsonPropertyName("agent")] public string? Agent { get; init; }
    /// <summary>Model override.</summary>
    [JsonPropertyName("model")] public string? Model { get; init; }
    /// <summary>Argument string for the command.</summary>
    [JsonPropertyName("arguments")] public string Arguments { get; init; } = string.Empty;
    /// <summary>Command name.</summary>
    [JsonPropertyName("command")] public string Command { get; init; } = string.Empty;
}

/// <summary>Body for <c>POST /session/{id}/shell</c>.</summary>
public sealed record SessionShellRequest
{
    /// <summary>Agent name to invoke.</summary>
    [JsonPropertyName("agent")] public string Agent { get; init; } = string.Empty;
    /// <summary>Optional model override.</summary>
    [JsonPropertyName("model")] public ModelRef? Model { get; init; }
    /// <summary>Shell command to execute.</summary>
    [JsonPropertyName("command")] public string Command { get; init; } = string.Empty;
}

/// <summary>Body for <c>POST /session/{id}/revert</c>.</summary>
public sealed record SessionRevertRequest
{
    /// <summary>Identifier of the message to revert to.</summary>
    [JsonPropertyName("messageID")] public string MessageId { get; init; } = string.Empty;
    /// <summary>Optional part id within the message.</summary>
    [JsonPropertyName("partID")] public string? PartId { get; init; }
}

/// <summary>Body for <c>POST /session/{id}/permissions/{permissionID}</c>.</summary>
public sealed record PermissionReplyRequest
{
    /// <summary>User response: <c>once</c>, <c>always</c>, or <c>reject</c>.</summary>
    [JsonPropertyName("response")] public PermissionResponse Response { get; init; }
}

/// <summary>Discriminated union of part inputs for prompts.</summary>
[JsonConverter(typeof(PartInputConverter))]
public sealed record PartInput
{
    internal PartInput(string type, JsonElement raw)
    {
        Type = type;
        Raw = raw;
    }

    /// <summary>Discriminator.</summary>
    public string Type { get; }

    /// <summary>Raw JSON element.</summary>
    public JsonElement Raw { get; }

    /// <summary>Builds a text part input.</summary>
    public static PartInput Text(string text, string? id = null)
    {
        var json = id is null
            ? (JsonElement?)null
            : JsonSerializer.SerializeToElement(new { id, type = "text", text });
        var element = json ?? JsonSerializer.SerializeToElement(new { type = "text", text });
        return new PartInput("text", element);
    }

    /// <summary>Builds a file part input.</summary>
    public static PartInput File(string mime, string url, string? filename = null)
    {
        var element = JsonSerializer.SerializeToElement(new { type = "file", mime, url, filename });
        return new PartInput("file", element);
    }

    /// <summary>Builds an agent part input.</summary>
    public static PartInput Agent(string name, SourceSpan? source = null)
    {
        var element = JsonSerializer.SerializeToElement(new { type = "agent", name, source });
        return new PartInput("agent", element);
    }

    /// <summary>Builds a subtask part input.</summary>
    public static PartInput Subtask(string prompt, string description, string agent)
    {
        var element = JsonSerializer.SerializeToElement(new { type = "subtask", prompt, description, agent });
        return new PartInput("subtask", element);
    }
}
