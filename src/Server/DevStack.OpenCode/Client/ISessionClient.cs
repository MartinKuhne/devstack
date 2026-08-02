using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

/// <summary>Session operations from the OpenCode SDK. Mirrors <c>client.session.*</c>.</summary>
public interface ISessionClient
{
    /// <summary>List all sessions (<c>GET /session</c>).</summary>
    Task<IReadOnlyList<Session>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Create a new session (<c>POST /session</c>).</summary>
    Task<Session> CreateAsync(SessionCreateRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Get session status for all sessions (<c>GET /session/status</c>).</summary>
    Task<IReadOnlyDictionary<string, SessionStatusInfo>> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Delete a session and all its data (<c>DELETE /session/{id}</c>).</summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Get a session by id (<c>GET /session/{id}</c>).</summary>
    Task<Session> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Update session properties (<c>PATCH /session/{id}</c>).</summary>
    Task<Session> UpdateAsync(string id, SessionUpdateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Get a session's children (<c>GET /session/{id}/children</c>).</summary>
    Task<IReadOnlyList<Session>> GetChildrenAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Get the todo list for a session (<c>GET /session/{id}/todo</c>).</summary>
    Task<IReadOnlyList<Todo>> GetTodosAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Analyze the app and create an <c>AGENTS.md</c> file (<c>POST /session/{id}/init</c>).</summary>
    Task<bool> InitAsync(string id, SessionInitRequest request, CancellationToken cancellationToken = default);

    /// <summary>Fork an existing session at a specific message (<c>POST /session/{id}/fork</c>).</summary>
    Task<Session> ForkAsync(string id, SessionForkRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Abort a session (<c>POST /session/{id}/abort</c>).</summary>
    Task<bool> AbortAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Unshare a session (<c>DELETE /session/{id}/share</c>).</summary>
    Task<Session> UnshareAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Share a session (<c>POST /session/{id}/share</c>).</summary>
    Task<Session> ShareAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Get the diff for a session (<c>GET /session/{id}/diff</c>).</summary>
    Task<IReadOnlyList<FileDiff>> GetDiffAsync(string id, string? messageId = null, CancellationToken cancellationToken = default);

    /// <summary>Summarize the session (<c>POST /session/{id}/summarize</c>).</summary>
    Task<bool> SummarizeAsync(string id, SessionSummarizeRequest request, CancellationToken cancellationToken = default);

    /// <summary>List messages for a session (<c>GET /session/{id}/message</c>).</summary>
    Task<IReadOnlyList<SessionMessageView>> GetMessagesAsync(string id, int? limit = null, CancellationToken cancellationToken = default);

    /// <summary>Create and send a new prompt to a session (<c>POST /session/{id}/message</c>).</summary>
    Task<SessionMessageView> PromptAsync(string id, SessionPromptRequest request, CancellationToken cancellationToken = default);

    /// <summary>Get a single message from a session (<c>GET /session/{id}/message/{messageID}</c>).</summary>
    Task<SessionMessageView> GetMessageAsync(string id, string messageId, CancellationToken cancellationToken = default);

    /// <summary>Send a prompt and return immediately (<c>POST /session/{id}/prompt_async</c>).</summary>
    Task PromptAsyncFireAndForget(string id, SessionPromptRequest request, CancellationToken cancellationToken = default);

    /// <summary>Send a new command to a session (<c>POST /session/{id}/command</c>).</summary>
    Task<SessionMessageView> CommandAsync(string id, SessionCommandRequest request, CancellationToken cancellationToken = default);

    /// <summary>Run a shell command (<c>POST /session/{id}/shell</c>).</summary>
    Task<AssistantMessage> ShellAsync(string id, SessionShellRequest request, CancellationToken cancellationToken = default);

    /// <summary>Revert a message (<c>POST /session/{id}/revert</c>).</summary>
    Task<Session> RevertAsync(string id, SessionRevertRequest request, CancellationToken cancellationToken = default);

    /// <summary>Restore all reverted messages (<c>POST /session/{id}/unrevert</c>).</summary>
    Task<Session> UnrevertAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Respond to a permission request (<c>POST /session/{id}/permissions/{permissionID}</c>).</summary>
    Task<bool> ReplyToPermissionAsync(string id, string permissionId, PermissionReplyRequest request, CancellationToken cancellationToken = default);
}

/// <summary>A message along with its parts as returned by the session message endpoints.</summary>
public sealed record SessionMessageView
{
    /// <summary>The message metadata.</summary>
    [JsonPropertyName("info")]
    public Message Info { get; init; } = new("unknown", default);

    /// <summary>The message parts.</summary>
    [JsonPropertyName("parts")]
    public IReadOnlyList<Part> Parts { get; init; } = Array.Empty<Part>();
}
