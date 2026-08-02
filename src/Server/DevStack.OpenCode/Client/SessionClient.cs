using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

internal sealed class SessionClient : ISessionClient
{
    private readonly OpenCodeHttp _http;
    public SessionClient(OpenCodeHttp http) => _http = http;

    public async Task<IReadOnlyList<Session>> ListAsync(CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<Session>>("session", cancellationToken: cancellationToken).ConfigureAwait(false);

    public async Task<Session> CreateAsync(SessionCreateRequest? request = null, CancellationToken cancellationToken = default) =>
        await _http.PostAsync<SessionCreateRequest?, Session>("session", request, cancellationToken: cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyDictionary<string, SessionStatusInfo>> GetStatusAsync(CancellationToken cancellationToken = default) =>
        await _http.GetAsync<Dictionary<string, SessionStatusInfo>>("session/status", cancellationToken: cancellationToken).ConfigureAwait(false);

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        _http.DeleteAsync($"session/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    public Task<Session> GetAsync(string id, CancellationToken cancellationToken = default) =>
        _http.GetAsync<Session>($"session/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    public Task<Session> UpdateAsync(string id, SessionUpdateRequest request, CancellationToken cancellationToken = default) =>
        _http.PatchAsync<SessionUpdateRequest, Session>($"session/{Uri.EscapeDataString(id)}", request, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<Session>> GetChildrenAsync(string id, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<Session>>($"session/{Uri.EscapeDataString(id)}/children", cancellationToken: cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Todo>> GetTodosAsync(string id, CancellationToken cancellationToken = default) =>
        await _http.GetAsync<List<Todo>>($"session/{Uri.EscapeDataString(id)}/todo", cancellationToken: cancellationToken).ConfigureAwait(false);

    public Task<bool> InitAsync(string id, SessionInitRequest request, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync($"session/{Uri.EscapeDataString(id)}/init", request, cancellationToken: cancellationToken);

    public Task<Session> ForkAsync(string id, SessionForkRequest? request = null, CancellationToken cancellationToken = default) =>
        _http.PostAsync<SessionForkRequest?, Session>($"session/{Uri.EscapeDataString(id)}/fork", request, cancellationToken: cancellationToken);

    public Task<bool> AbortAsync(string id, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync<object?>($"session/{Uri.EscapeDataString(id)}/abort", null, cancellationToken: cancellationToken);

    public Task<Session> UnshareAsync(string id, CancellationToken cancellationToken = default) =>
        _http.DeleteAsync<Session>($"session/{Uri.EscapeDataString(id)}/share", cancellationToken: cancellationToken);

    public Task<Session> ShareAsync(string id, CancellationToken cancellationToken = default) =>
        _http.PostAsync<object?, Session>($"session/{Uri.EscapeDataString(id)}/share", null, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<FileDiff>> GetDiffAsync(string id, string? messageId = null, CancellationToken cancellationToken = default)
    {
        var query = messageId is null ? null : new Dictionary<string, string?> { ["messageID"] = messageId };
        return await _http.GetAsync<List<FileDiff>>($"session/{Uri.EscapeDataString(id)}/diff", query, cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> SummarizeAsync(string id, SessionSummarizeRequest request, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync($"session/{Uri.EscapeDataString(id)}/summarize", request, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<SessionMessageView>> GetMessagesAsync(string id, int? limit = null, CancellationToken cancellationToken = default)
    {
        var query = limit is null ? null : new Dictionary<string, string?> { ["limit"] = limit.Value.ToString() };
        return await _http.GetAsync<List<SessionMessageView>>($"session/{Uri.EscapeDataString(id)}/message", query, cancellationToken).ConfigureAwait(false);
    }

    public Task<SessionMessageView> PromptAsync(string id, SessionPromptRequest request, CancellationToken cancellationToken = default) =>
        _http.PostAsync<SessionPromptRequest, SessionMessageView>($"session/{Uri.EscapeDataString(id)}/message", request, cancellationToken: cancellationToken);

    public Task<SessionMessageView> GetMessageAsync(string id, string messageId, CancellationToken cancellationToken = default) =>
        _http.GetAsync<SessionMessageView>($"session/{Uri.EscapeDataString(id)}/message/{Uri.EscapeDataString(messageId)}", cancellationToken: cancellationToken);

    public Task PromptAsyncFireAndForget(string id, SessionPromptRequest request, CancellationToken cancellationToken = default) =>
        _http.PostNoContentAsync($"session/{Uri.EscapeDataString(id)}/prompt_async", request, cancellationToken: cancellationToken);

    public Task<SessionMessageView> CommandAsync(string id, SessionCommandRequest request, CancellationToken cancellationToken = default) =>
        _http.PostAsync<SessionCommandRequest, SessionMessageView>($"session/{Uri.EscapeDataString(id)}/command", request, cancellationToken: cancellationToken);

    public Task<AssistantMessage> ShellAsync(string id, SessionShellRequest request, CancellationToken cancellationToken = default) =>
        _http.PostAsync<SessionShellRequest, AssistantMessage>($"session/{Uri.EscapeDataString(id)}/shell", request, cancellationToken: cancellationToken);

    public Task<Session> RevertAsync(string id, SessionRevertRequest request, CancellationToken cancellationToken = default) =>
        _http.PostAsync<SessionRevertRequest, Session>($"session/{Uri.EscapeDataString(id)}/revert", request, cancellationToken: cancellationToken);

    public Task<Session> UnrevertAsync(string id, CancellationToken cancellationToken = default) =>
        _http.PostAsync<object?, Session>($"session/{Uri.EscapeDataString(id)}/unrevert", null, cancellationToken: cancellationToken);

    public Task<bool> ReplyToPermissionAsync(string id, string permissionId, PermissionReplyRequest request, CancellationToken cancellationToken = default) =>
        _http.PostBoolAsync($"session/{Uri.EscapeDataString(id)}/permissions/{Uri.EscapeDataString(permissionId)}", request, cancellationToken: cancellationToken);
}
