using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

/// <summary>TUI operations (<c>client.tui.*</c>).</summary>
public interface ITuiClient
{
    /// <summary>Append text to the prompt (<c>POST /tui/append-prompt</c>).</summary>
    Task<bool> AppendPromptAsync(string text, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Open the help dialog (<c>POST /tui/open-help</c>).</summary>
    Task<bool> OpenHelpAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Open the session selector (<c>POST /tui/open-sessions</c>).</summary>
    Task<bool> OpenSessionsAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Open the theme selector (<c>POST /tui/open-themes</c>).</summary>
    Task<bool> OpenThemesAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Open the model selector (<c>POST /tui/open-models</c>).</summary>
    Task<bool> OpenModelsAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Submit the current prompt (<c>POST /tui/submit-prompt</c>).</summary>
    Task<bool> SubmitPromptAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Clear the prompt (<c>POST /tui/clear-prompt</c>).</summary>
    Task<bool> ClearPromptAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Execute a TUI command (e.g. <c>agent_cycle</c>) (<c>POST /tui/execute-command</c>).</summary>
    Task<bool> ExecuteCommandAsync(string command, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Show toast notification (<c>POST /tui/show-toast</c>).</summary>
    Task<bool> ShowToastAsync(TuiToastRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Publish a TUI event (<c>POST /tui/publish</c>).</summary>
    Task<bool> PublishAsync(TuiPublishRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Sub-client for the request/response control loop.</summary>
    ITuiControlClient Control { get; }
}

/// <summary>Request body for <c>POST /tui/show-toast</c>.</summary>
public sealed record TuiToastRequest
{
    /// <summary>Optional toast title.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }
    /// <summary>Toast message.</summary>
    [JsonPropertyName("message")] public string Message { get; init; } = string.Empty;
    /// <summary>Toast variant — <c>info</c>, <c>success</c>, <c>warning</c>, or <c>error</c>.</summary>
    [JsonPropertyName("variant")] public string Variant { get; init; } = "info";
    /// <summary>Duration in milliseconds.</summary>
    [JsonPropertyName("duration")] public int? Duration { get; init; }
}

/// <summary>Request body for <c>POST /tui/publish</c>.</summary>
public sealed record TuiPublishRequest
{
    /// <summary>Prompt-append event.</summary>
    [JsonPropertyName("promptAppend")] public TuiPromptAppendEvent? PromptAppend { get; init; }
    /// <summary>Command-execute event.</summary>
    [JsonPropertyName("commandExecute")] public TuiCommandExecuteEvent? CommandExecute { get; init; }
    /// <summary>Toast event.</summary>
    [JsonPropertyName("toastShow")] public TuiToastShowEvent? ToastShow { get; init; }
}

/// <summary>TUI prompt-append event.</summary>
public sealed record TuiPromptAppendEvent
{
    /// <summary>Text to append.</summary>
    [JsonPropertyName("text")] public string Text { get; init; } = string.Empty;
}

/// <summary>TUI command-execute event.</summary>
public sealed record TuiCommandExecuteEvent
{
    /// <summary>Command name (e.g. <c>prompt.submit</c>).</summary>
    [JsonPropertyName("command")] public string Command { get; init; } = string.Empty;
}

/// <summary>TUI toast-show event.</summary>
public sealed record TuiToastShowEvent
{
    /// <summary>Optional title.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }
    /// <summary>Message body.</summary>
    [JsonPropertyName("message")] public string Message { get; init; } = string.Empty;
    /// <summary>Variant — <c>info</c>, <c>success</c>, <c>warning</c>, or <c>error</c>.</summary>
    [JsonPropertyName("variant")] public string Variant { get; init; } = "info";
    /// <summary>Duration in milliseconds.</summary>
    [JsonPropertyName("duration")] public int? Duration { get; init; }
}

/// <summary>TUI request/response control loop (<c>client.tui.control.*</c>).</summary>
public interface ITuiControlClient
{
    /// <summary>Get the next TUI request from the queue (<c>GET /tui/control/next</c>).</summary>
    Task<TuiControlRequest> NextAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>Submit a response to the TUI request queue (<c>POST /tui/control/response</c>).</summary>
    Task<bool> SubmitResponseAsync(JsonElement body, string? directory = null, CancellationToken cancellationToken = default);
}

/// <summary>Single TUI control request from the queue.</summary>
public sealed record TuiControlRequest
{
    /// <summary>Request path.</summary>
    [JsonPropertyName("path")] public string Path { get; init; } = string.Empty;
    /// <summary>Request body as raw JSON.</summary>
    [JsonPropertyName("body")] public JsonElement Body { get; init; }
}
