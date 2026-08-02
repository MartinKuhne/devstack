using DevStack.OpenCode.Client;
using DevStack.OpenCode.Models;
using DevStack.OpenCode.Serialization;

using Microsoft.Extensions.Logging;

namespace DevStack.Agent;

/// <summary>
/// Thin CLI wrapper around <see cref="IOpenCodeClient"/> that creates a
/// session, sends a single prompt, and prints the assistant's reply.
/// Used as a smoke test against a running <c>opencode serve</c> instance.
/// </summary>
public sealed class OpenCodeAgent
{
    private readonly IOpenCodeClient _client;
    private readonly ILogger<OpenCodeAgent> _logger;

    /// <summary>Builds the agent with the OpenCode SDK client and a logger.</summary>
    public OpenCodeAgent(IOpenCodeClient client, ILogger<OpenCodeAgent> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a session, sends <paramref name="prompt"/> to it, and prints
    /// every part of the assistant's reply. Returns the session id so the
    /// caller can continue the conversation later.
    /// </summary>
    public async Task<string> RunAsync(
        string prompt,
        ModelRef? model = null,
        string? title = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        _logger.LogInformation("Checking OpenCode server health…");
        var health = await _client.GetHealthAsync(cancellationToken).ConfigureAwait(false);
        if (!health.Healthy)
        {
            throw new InvalidOperationException(
                $"OpenCode server reports unhealthy state. Version: {health.Version ?? "<unknown>"}. " +
                "Is `opencode serve` running on the configured base URL?");
        }

        var listing = await ListProvidersAsync(cancellationToken).ConfigureAwait(false);

        var resolvedModel = ResolveModel(model, listing);

        WarnIfModelUnavailable(resolvedModel, listing);

        _logger.LogInformation("Creating session (title={Title})", title ?? "<auto>");
        var session = await _client.Session.CreateAsync(
            new SessionCreateRequest { Title = title ?? $"DevStack.Agent @ {DateTimeOffset.UtcNow:o}" },
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Session created: {SessionId}", session.Id);

        _logger.LogInformation("Sending prompt to {Provider}/{Model}…", resolvedModel.ProviderId, resolvedModel.ModelId);

        // Stream the live transcript from the global SSE channel so the
        // operator sees each message and part as the model emits them —
        // not just the final reply. The subscription starts before
        // PromptAsync so we don't miss the opening message.updated /
        // part.updated events, and ends on session.idle (which the server
        // emits just after the assistant message is finalised).
        using var streamCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var streamTask = StreamSessionAsync(session.Id, streamCts.Token);

        // Heartbeat is now a safety net: live streaming already shows the
        // model working, but if the SSE stream stalls (e.g. a server hiccup
        // during a long tool call) the 30s heartbeat gives the operator a
        // "still alive" signal.
        using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var heartbeat = HeartbeatAsync(heartbeatCts.Token, resolvedModel, session.Id);

        SessionMessageView result;
        try
        {
            result = await _client.Session.PromptAsync(
                session.Id,
                new SessionPromptRequest
                {
                    Model = resolvedModel,
                    Parts = new[] { PartInput.Text(prompt) },
                },
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            heartbeatCts.Cancel();
            try { await heartbeat.ConfigureAwait(false); } catch { /* swallow on shutdown */ }
        }

        // PromptAsync returns when the assistant message is finalised, but
        // the server-side session goes idle slightly after. Wait briefly
        // for the stream consumer to drain the closing events
        // (session.idle + the final part.updated), then force-cancel if
        // it's still running.
        using var drainCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        using var reg = drainCts.Token.Register(() => streamCts.Cancel());
        try
        {
            await streamTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { /* expected on shutdown */ }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Live transcript stream ended with an exception.");
        }

        _logger.LogInformation(
            "Prompt response received for session {SessionId}: {PartCount} part(s) in the final message, info kind={Kind}.",
            session.Id, result.Parts.Count, result.Info.Kind);

        // The stream consumer has already printed every message and part
        // live. The only thing left is the per-run summary block.
        PrintRunSummary(result);

        return session.Id;
    }

    /// <summary>
    /// Logs a "still waiting" line every 30 seconds while a long-running
    /// OpenCode call is in flight, so the operator can see the agent is
    /// alive instead of wondering whether the LLM is hung. The cadence is
    /// hard-coded for now; if we ever need to make it configurable the
    /// right place is the OpenCode section of appsettings.
    /// </summary>
    private async Task HeartbeatAsync(CancellationToken cancellationToken, ModelRef model, string sessionId)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var nextBeatAt = startedAt + TimeSpan.FromSeconds(30);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(nextBeatAt - DateTimeOffset.UtcNow, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                var elapsed = DateTimeOffset.UtcNow - startedAt;
                _logger.LogInformation(
                    "Still waiting on {Provider}/{Model} for session {SessionId}… ({ElapsedSeconds:F0}s elapsed)",
                    model.ProviderId, model.ModelId, sessionId, elapsed.TotalSeconds);

                nextBeatAt += TimeSpan.FromSeconds(30);
            }
        }
        finally
        {
            // Tell the operator the wait is over (e.g. response received or
            // cancelled). Keeps the log trail clear when the call returned
            // within a few hundred ms and the user only saw "Sending…".
            var elapsed = DateTimeOffset.UtcNow - startedAt;
            _logger.LogDebug(
                "Heartbeat stopped for session {SessionId} after {ElapsedSeconds:F1}s.",
                sessionId, elapsed.TotalSeconds);
        }
    }

    private void PrintReply(SessionMessageView result)
    {
        // Kept for callers that only have the final message. The full
        // transcript (with thinking + tool calls) is printed by
        // PrintRunTranscriptAsync, which is what RunAsync calls now.
        Console.WriteLine();
        Console.WriteLine($"--- assistant reply ({result.Info.Kind}) ---");

        if (result.Parts.Count == 0)
        {
            Console.WriteLine("<no parts returned>");
            return;
        }

        foreach (var part in result.Parts)
        {
            switch (part.Kind)
            {
                case "text":
                    Console.WriteLine(part.AsText().Text);
                    break;

                case "reasoning":
                    Console.WriteLine($"  [thinking] {part.AsReasoning().Text}");
                    break;

                case "tool":
                    var tool = part.AsTool();
                    Console.WriteLine($"  [tool] {tool.Tool} ({tool.State?.Status ?? "unknown"})");
                    break;

                case "file":
                    var file = part.AsFile();
                    Console.WriteLine($"  [file] {file.Mime} {file.Filename ?? file.Url}");
                    break;

                case "step-start":
                    Console.WriteLine("  [step start]");
                    break;

                case "step-finish":
                    var finish = part.AsStepFinish();
                    Console.WriteLine($"  [step finish] reason={finish.Reason} cost=${finish.Cost}");
                    break;

                default:
                    Console.WriteLine($"  [{part.Kind}] <unhandled>");
                    break;
            }
        }

        if (result.Info.IsAssistant)
        {
            var assistant = result.Info.AsAssistant();
            Console.WriteLine();
            Console.WriteLine($"model:    {assistant.ProviderId}/{assistant.ModelId}");
            Console.WriteLine($"tokens:   in={assistant.Tokens.Input} out={assistant.Tokens.Output} " +
                              $"reasoning={assistant.Tokens.Reasoning} cache.read={assistant.Tokens.Cache.Read} cache.write={assistant.Tokens.Cache.Write}");
            Console.WriteLine($"cost:     ${assistant.Cost:F4}");
            if (assistant.Finish is { Length: > 0 } finish)
            {
                Console.WriteLine($"finish:   {finish}");
            }
        }

        Console.WriteLine("--- end ---");
    }

    /// <summary>
    /// Prints just the per-run summary block (model, tokens, cost, finish)
    /// from the final assistant message returned by <c>PromptAsync</c>. The
    /// streaming consumer prints every message and part live; this is the
    /// one piece that's only available after the prompt returns.
    /// </summary>
    private void PrintRunSummary(SessionMessageView result)
    {
        if (!result.Info.IsAssistant)
        {
            return;
        }

        var assistant = result.Info.AsAssistant();
        Console.WriteLine();
        Console.WriteLine("--- run summary ---");
        Console.WriteLine($"model:    {assistant.ProviderId}/{assistant.ModelId}");
        Console.WriteLine($"tokens:   in={assistant.Tokens.Input} out={assistant.Tokens.Output} " +
                          $"reasoning={assistant.Tokens.Reasoning} cache.read={assistant.Tokens.Cache.Read} cache.write={assistant.Tokens.Cache.Write}");
        Console.WriteLine($"cost:     ${assistant.Cost:F4}");
        if (assistant.Finish is { Length: > 0 } finish)
        {
            Console.WriteLine($"finish:   {finish}");
        }
        Console.WriteLine("--- end of session ---");
    }

    /// <summary>
    /// Subscribes to the global <c>/global/event</c> SSE stream and prints
    /// every event related to <paramref name="sessionId"/> as it arrives:
    /// per-message header, then each part with the same icon legend used
    /// by the post-hoc transcript (reasoning, tool, file, step-*, etc.).
    /// Text and reasoning parts stream in via <c>message.part.delta</c>
    /// events; on the first delta of a part we print a single
    /// <c>💭 …</c> placeholder so the operator knows the model is still
    /// working, and on the canonical <c>message.part.updated</c> that
    /// follows the deltas we print the full text on a new line. We do
    /// not render the deltas themselves — emitting hundreds of
    /// truncated partial lines is far worse than a single placeholder,
    /// and <c>\r</c>-based overwriting is fragile in any non-TTY
    /// capture (pipe, file, screen reader, copy-paste buffer).
    /// </summary>
    /// <remarks>
    /// The consumer returns when the server emits <c>session.idle</c> (the
    /// run is done), <c>session.error</c>, or when the cancellation token
    /// fires (PromptAsync returned and the drain timeout elapsed).
    /// </remarks>
    private async Task StreamSessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        var seenMessageIds = new HashSet<string>(StringComparer.Ordinal);
        var partKinds = new Dictionary<string, string>(StringComparer.Ordinal);
        var streamingPartIds = new HashSet<string>(StringComparer.Ordinal);
        var printedTextParts = new HashSet<string>(StringComparer.Ordinal);

        try
        {
            await foreach (var globalEvent in _client.Global.SubscribeAsync(cancellationToken).ConfigureAwait(false))
            {
                var payload = globalEvent.Payload;
                if (payload?.Properties is not { } props)
                {
                    continue;
                }

                // The global stream carries events for every active session,
                // not just ours. Filter on sessionID so we only react to
                // events for the run we kicked off.
                if (!props.TryGetProperty("sessionID", out var sidEl)
                    || sidEl.GetString() != sessionId)
                {
                    continue;
                }

                switch (payload.Type)
                {
                    case "server.connected":
                    case "server.heartbeat":
                    case "sync":
                    case "session.created":
                    case "session.updated":
                    case "session.diff":
                    case "session.status":
                        // Bookkeeping only — no per-line output.
                        break;

                    case "message.updated":
                        if (props.TryGetProperty("info", out var infoEl))
                        {
                            var info = DeserializeFromJson<Message>(infoEl);
                            OnMessageUpdated(info, seenMessageIds);
                        }
                        break;

                    case "message.part.updated":
                        if (props.TryGetProperty("part", out var partEl))
                        {
                            var part = DeserializeFromJson<Part>(partEl);
                            OnPartUpdated(part, partKinds, streamingPartIds, printedTextParts);
                        }
                        break;

                    case "message.part.delta":
                        if (props.TryGetProperty("partID", out var partIdEl)
                            && partIdEl.GetString() is { Length: > 0 } partId
                            && props.TryGetProperty("field", out var fieldEl)
                            && fieldEl.GetString() == "text"
                            && props.TryGetProperty("delta", out var deltaEl))
                        {
                            var delta = deltaEl.GetString() ?? string.Empty;
                            OnTextDelta(partId, delta, partKinds, streamingPartIds);
                        }
                        break;

                    case "session.idle":
                        return;

                    case "session.error":
                        if (props.TryGetProperty("error", out var errEl))
                        {
                            _logger.LogError(
                                "OpenCode session {SessionId} reported an error event: {Error}",
                                sessionId, errEl.ToString());
                        }
                        else
                        {
                            _logger.LogError(
                                "OpenCode session {SessionId} reported an error event.", sessionId);
                        }
                        return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when PromptAsync returns and the drain timeout elapses.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Live transcript stream for session {SessionId} ended unexpectedly; " +
                "the run summary will still be printed below.",
                sessionId);
        }
    }

    private void OnMessageUpdated(
        Message info,
        HashSet<string> seenMessageIds)
    {
        // Base Message doesn't expose Id (it lives on UserMessage /
        // AssistantMessage). Read the raw id field — always present on the
        // wire — and use it to dedupe across the many message.updated
        // events the server emits for the same message.
        var id = info.Raw.ValueKind == System.Text.Json.JsonValueKind.Object
                 && info.Raw.TryGetProperty("id", out var idEl)
                 && idEl.ValueKind == System.Text.Json.JsonValueKind.String
            ? idEl.GetString() ?? string.Empty
            : string.Empty;

        if (string.IsNullOrEmpty(id) || !seenMessageIds.Add(id))
        {
            return;
        }

        var headerExtras = BuildMessageHeaderExtras(info);
        Console.WriteLine();
        Console.WriteLine($"── msg {seenMessageIds.Count} (role={info.Kind}{headerExtras}) ──");
    }

    private void OnPartUpdated(
        Part part,
        Dictionary<string, string> partKinds,
        HashSet<string> streamingPartIds,
        HashSet<string> printedTextParts)
    {
        var partId = part.Id;
        if (string.IsNullOrEmpty(partId))
        {
            return;
        }

        partKinds[partId] = part.Kind;

        if (part.Kind is "text" or "reasoning")
        {
            // Text/reasoning parts come in three phases:
            //   1. first part.updated with empty/initial text,
            //   2. many message.part.delta events streaming the chunks,
            //   3. a final part.updated carrying the canonical text.
            // We wait for phase 3 (signalled by at least one delta
            // having been seen) and then print the full text exactly
            // once. The placeholder emitted by OnTextDelta gives the
            // operator visibility during phase 2; we never render the
            // deltas themselves, so a 200-char thinking line doesn't
            // produce 200 partial lines in the output.
            if (streamingPartIds.Contains(partId) && printedTextParts.Add(partId))
            {
                var text = part.Kind == "text" ? part.AsText().Text : part.AsReasoning().Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var prefix = part.Kind == "reasoning" ? "  💭 " : string.Empty;
                    Console.WriteLine(prefix + text);
                }
            }
        }
        else
        {
            // Structural parts (tool, file, step-*, patch, subtask, …) get
            // a fresh render on every update so the operator sees status
            // changes (e.g. tool: running → completed).
            PrintPart(part);
        }
    }

    private void OnTextDelta(
        string partId,
        string delta,
        Dictionary<string, string> partKinds,
        HashSet<string> streamingPartIds)
    {
        if (string.IsNullOrEmpty(delta))
        {
            return;
        }

        // On the first delta for a part, print a single placeholder
        // (💭 … for reasoning, … for text) so the operator knows the
        // model is still working. Subsequent deltas are dropped — the
        // canonical text is printed by OnPartUpdated when the server's
        // final part.updated arrives, and we don't want to flood the
        // console with hundreds of partial lines (especially because
        // \r-based in-place overwriting is fragile in any non-TTY
        // capture — pipes, files, screen readers, copy-paste buffers).
        if (streamingPartIds.Add(partId))
        {
            var kind = partKinds.TryGetValue(partId, out var k) ? k : "text";
            Console.WriteLine(kind == "reasoning" ? "  💭 …" : "  …");
        }
    }

    /// <summary>
    /// Prints a single part of the transcript. Reasoning and tool parts
    /// are previews (truncated) so a single tool call with a 5k-character
    /// input doesn't blow up the console; the user can read the full
    /// message via the OpenCode server UI.
    /// </summary>
    private static void PrintPart(Part part)
    {
        switch (part.Kind)
        {
            case "text":
            {
                var text = part.AsText().Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine(text);
                }
                break;
            }

            case "reasoning":
            {
                var reasoning = part.AsReasoning().Text;
                if (!string.IsNullOrWhiteSpace(reasoning))
                {
                    var preview = Truncate(reasoning, 500);
                    Console.WriteLine($"  💭 {preview}");
                }
                break;
            }

            case "tool":
            {
                var tool = part.AsTool();
                var status = tool.State?.Status ?? "unknown";
                var icon = status switch
                {
                    "completed" => "✓",
                    "error" => "✗",
                    "running" => "⏳",
                    "pending" => "…",
                    _ => "•",
                };

                var inputPreview = ReadJsonField(tool.State?.Raw, "input");
                var inputLine = Truncate(inputPreview, 240);

                Console.WriteLine($"  {icon} 🔧 {tool.Tool} ({status})");
                if (!string.IsNullOrWhiteSpace(inputLine))
                {
                    Console.WriteLine($"     in:  {inputLine}");
                }

                var outputPreview = ReadJsonField(tool.State?.Raw, "output");
                var outputLine = Truncate(outputPreview, 240);
                if (!string.IsNullOrWhiteSpace(outputLine) && outputLine != "null")
                {
                    Console.WriteLine($"     out: {outputLine}");
                }
                break;
            }

            case "file":
            {
                var file = part.AsFile();
                var name = file.Filename ?? file.Url;
                Console.WriteLine($"  📄 {file.Mime} {name}");
                break;
            }

            case "patch":
            {
                var patch = part.AsPatch();
                var files = patch.Files.Count == 0
                    ? "<no files>"
                    : string.Join(", ", patch.Files);
                Console.WriteLine($"  🔧 patch ({patch.Files.Count} file(s)): {files}");
                break;
            }

            case "step-start":
            {
                // Implicit — the message header already shows the turn number.
                // Still emit a marker so users can grep for "step-start" if needed.
                Console.WriteLine("  ── step start ──");
                break;
            }

            case "step-finish":
            {
                var finish = part.AsStepFinish();
                var detailParts = new List<string>();
                if (!string.IsNullOrEmpty(finish.Reason))
                {
                    detailParts.Add($"reason={finish.Reason}");
                }
                if (finish.Cost > 0)
                {
                    detailParts.Add($"cost=${finish.Cost:F4}");
                }
                var tokens = finish.Tokens;
                if ((tokens.Input + tokens.Output + tokens.Reasoning) > 0)
                {
                    detailParts.Add($"tokens=in:{tokens.Input} out:{tokens.Output} reasoning:{tokens.Reasoning}");
                }
                var detail = detailParts.Count == 0
                    ? string.Empty
                    : " " + string.Join(" ", detailParts);
                Console.WriteLine($"  ── step finish{detail} ──");
                break;
            }

            case "subtask":
            {
                var subtask = part.AsSubtask();
                Console.WriteLine($"  👥 subtask → agent={subtask.Agent}: {Truncate(subtask.Prompt, 160)}");
                break;
            }

            case "agent":
            {
                var agent = part.AsAgent();
                Console.WriteLine($"  👤 agent: {agent.Name}");
                break;
            }

            case "snapshot":
            {
                var snap = part.AsSnapshot();
                Console.WriteLine($"  📸 snapshot {snap.Snapshot}");
                break;
            }

            case "retry":
            {
                var retry = part.AsRetry();
                var errText = retry.Error.ValueKind == System.Text.Json.JsonValueKind.String
                    ? retry.Error.GetString() ?? "<error>"
                    : retry.Error.ToString();
                Console.WriteLine($"  🔁 retry attempt={retry.Attempt}: {Truncate(errText, 160)}");
                break;
            }

            case "compaction":
            {
                var comp = part.AsCompaction();
                var kind = comp.Auto ? "auto" : "manual";
                Console.WriteLine($"  🗜  compaction ({kind})");
                break;
            }

            default:
                Console.WriteLine($"  [{part.Kind}] <unhandled>");
                break;
        }
    }

    /// <summary>
    /// Deserializes a strongly-typed <typeparamref name="T"/> from a
    /// <see cref="System.Text.Json.JsonElement"/> using the OpenCode SDK's
    /// shared serializer options. The type-level
    /// <c>[JsonConverter(typeof(MessageConverter))]</c> and
    /// <c>[JsonConverter(typeof(PartConverter))]</c> attributes on
    /// <see cref="Message"/> and <see cref="Part"/> make the discriminated
    /// unions round-trip without a global converter registration.
    /// </summary>
    private static T DeserializeFromJson<T>(System.Text.Json.JsonElement element)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(element.GetRawText(), OpenCodeJson.Compact)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize {typeof(T).Name} from SSE event payload.");
    }

    /// <summary>
    /// Builds the optional <c>agent=…</c> and <c>model=…</c> suffix shown
    /// after the per-message role. The model and agent fields live on the
    /// assistant/user sub-types rather than on <see cref="Message"/> itself,
    /// so we dispatch on the discriminator instead of failing the cast.
    /// Empty string is returned when no useful extras are available.
    /// </summary>
    private static string BuildMessageHeaderExtras(Message info)
    {
        if (info.IsAssistant)
        {
            var assistant = info.AsAssistant();
            return (string.IsNullOrEmpty(assistant.ProviderId) && string.IsNullOrEmpty(assistant.ModelId))
                ? string.Empty
                : $" model={assistant.ProviderId}/{assistant.ModelId}";
        }

        if (info.IsUser)
        {
            var user = info.AsUser();
            var agentPart = string.IsNullOrEmpty(user.Agent) ? string.Empty : $" agent={user.Agent}";
            var modelPart = (string.IsNullOrEmpty(user.Model.ProviderId) && string.IsNullOrEmpty(user.Model.ModelId))
                ? string.Empty
                : $" model={user.Model.ProviderId}/{user.Model.ModelId}";
            return $"{agentPart}{modelPart}";
        }

        return string.Empty;
    }

    /// <summary>
    /// Reads a top-level field from a <see cref="System.Text.Json.JsonElement"/>
    /// and returns it as a string. Handles strings, numbers, objects, and
    /// arrays by re-serializing non-string values so the output is
    /// always readable on the console.
    /// </summary>
    private static string ReadJsonField(System.Text.Json.JsonElement? element, string fieldName)
    {
        if (!element.HasValue || element.Value.ValueKind != System.Text.Json.JsonValueKind.Object)
        {
            return string.Empty;
        }
        if (!element.Value.TryGetProperty(fieldName, out var field))
        {
            return string.Empty;
        }
        return field.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => field.GetString() ?? string.Empty,
            System.Text.Json.JsonValueKind.Null => string.Empty,
            System.Text.Json.JsonValueKind.Number or
            System.Text.Json.JsonValueKind.True or
            System.Text.Json.JsonValueKind.False => field.ToString(),
            _ => System.Text.Json.JsonSerializer.Serialize(field, new System.Text.Json.JsonSerializerOptions { WriteIndented = false }),
        };
    }

    /// <summary>Truncates a string to <paramref name="maxLength"/> characters with an ellipsis if cut.</summary>
    private static string Truncate(string? s, int maxLength)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }
        return s.Length <= maxLength ? s : s.Substring(0, maxLength) + "…";
    }

    /// <summary>
    /// Fetches the provider/model inventory from the OpenCode server and
    /// pretty-prints the connected providers. Not-connected providers are
    /// filtered out of the printout and the auto-pick map; the raw response
    /// is still kept on the returned <see cref="ProviderListing"/> so
    /// <see cref="WarnIfModelUnavailable"/> can mention them when the user
    /// explicitly types a non-connected model. Returns <c>null</c> when the
    /// listing call fails; the caller then falls back to the hardcoded
    /// default model.
    /// </summary>
    private async Task<ProviderListing?> ListProvidersAsync(CancellationToken cancellationToken)
    {
        ProviderListResponse list;
        try
        {
            list = await _client.Provider.ListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to list providers and models from {BaseUrl}; continuing without the listing. " +
                "Use --model provider/model to pick a model explicitly.",
                _client.BaseUrl);
            return null;
        }

        var connected = new HashSet<string>(list.Connected, StringComparer.Ordinal);
        var visibleProviders = list.All.Where(p => connected.Contains(p.Id)).ToList();
        var available = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        if (visibleProviders.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine("Available providers (0 connected):");
            Console.WriteLine();
            Console.WriteLine("  No connected providers on the server. The prompt will likely fail; use");
            Console.WriteLine("  --model provider/model to pick a model, or configure a provider on the");
            Console.WriteLine("  OpenCode server (e.g. `opencode auth set <provider> <key>`).");
        }
        else
        {
            var hidden = list.All.Count - visibleProviders.Count;
            Console.WriteLine();
            Console.WriteLine(hidden > 0
                ? $"Available providers ({visibleProviders.Count} connected; {hidden} more not connected, hidden):"
                : $"Available providers ({visibleProviders.Count} connected):");

            foreach (var provider in visibleProviders)
            {
                var source = string.IsNullOrEmpty(provider.Source) ? "?" : provider.Source;
                list.Default.TryGetValue(provider.Id, out var defaultModelId);

                Console.WriteLine($"  {provider.Id}  [source={source}]  " +
                                  $"default: {(string.IsNullOrEmpty(defaultModelId) ? "(none)" : defaultModelId)}  " +
                                  $"({provider.Models.Count} models)");

                var ids = new HashSet<string>(StringComparer.Ordinal);
                foreach (var (modelId, model) in provider.Models)
                {
                    var isDefault = !string.IsNullOrEmpty(defaultModelId)
                                    && string.Equals(modelId, defaultModelId, StringComparison.Ordinal);
                    var marker = isDefault ? ">" : " ";
                    var display = string.IsNullOrEmpty(model.Name) || model.Name == modelId
                        ? modelId
                        : $"{modelId}  ({model.Name})";
                    Console.WriteLine($"    {marker} {provider.Id}/{display}");
                    ids.Add(modelId);
                }

                if (ids.Count > 0)
                {
                    available[provider.Id] = ids;
                }
            }

            // Print a "Server default" hint — the first connected provider's
            // configured default — so the user can see what the server would
            // pick on its own.
            foreach (var pid in connected)
            {
                if (list.Default.TryGetValue(pid, out var mid) && !string.IsNullOrEmpty(mid))
                {
                    Console.WriteLine();
                    Console.WriteLine($"  Server default: {pid}/{mid}");
                    break;
                }
            }
        }

        return new ProviderListing(list,
            available.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlySet<string>)kvp.Value, StringComparer.Ordinal));
    }

    /// <summary>
    /// Resolves the model to send the prompt to. Honours an explicit
    /// <paramref name="model"/> when supplied; otherwise picks a sensible
    /// default from the server's inventory, preferring a connected
    /// provider's first model whose id or name contains <c>free</c>. Falls
    /// back to the hardcoded Anthropic default when no inventory is
    /// available or no model can be picked.
    /// </summary>
    private ModelRef ResolveModel(ModelRef? requested, ProviderListing? listing)
    {
        if (requested is not null)
        {
            return requested;
        }

        var auto = listing is null ? null : ResolveDefaultModel(listing.List);
        if (auto is not null)
        {
            // listing is guaranteed non-null here because `auto` is only
            // returned by ResolveDefaultModel when listing was non-null.
            var matched = ContainsFree(auto.ModelId) || ContainsFree(GetModelName(listing!, auto));
            _logger.LogInformation(
                "No --model specified; auto-selected {Provider}/{Model} ({Reason}). Use --model provider/model to override.",
                auto.ProviderId, auto.ModelId,
                matched ? "first *free* model on a connected provider" : "first connected provider's default");
            return auto;
        }

        _logger.LogWarning(
            "No --model specified and no model could be auto-resolved from the server's inventory; " +
            "falling back to anthropic/claude-3-5-sonnet. Use --model provider/model to override.");
        return new ModelRef { ProviderId = "anthropic", ModelId = "claude-3-5-sonnet" };
    }

    /// <summary>
    /// Picks a default model from the server's inventory. Only considers
    /// connected providers — the agent has no way to drive a model it can't
    /// reach. Preference order:
    /// 1. first model whose id or name contains <c>free</c> on a connected provider;
    /// 2. first connected provider's configured default.
    /// Returns <c>null</c> when no connected provider is available.
    /// </summary>
    private static ModelRef? ResolveDefaultModel(ProviderListResponse list)
    {
        var connected = new HashSet<string>(list.Connected, StringComparer.Ordinal);

        // 1. "free" on a connected provider
        foreach (var provider in list.All)
        {
            if (!connected.Contains(provider.Id))
            {
                continue;
            }
            foreach (var (modelId, model) in provider.Models)
            {
                if (ContainsFree(modelId) || ContainsFree(model.Name))
                {
                    return new ModelRef { ProviderId = provider.Id, ModelId = modelId };
                }
            }
        }

        // 2. first connected provider's configured default
        foreach (var provider in list.All)
        {
            if (!connected.Contains(provider.Id))
            {
                continue;
            }
            if (list.Default.TryGetValue(provider.Id, out var defaultModelId)
                && !string.IsNullOrEmpty(defaultModelId))
            {
                return new ModelRef { ProviderId = provider.Id, ModelId = defaultModelId };
            }
        }

        return null;
    }

    /// <summary>True when <paramref name="s"/> contains the substring <c>free</c> (case-insensitive).</summary>
    private static bool ContainsFree(string? s) =>
        !string.IsNullOrEmpty(s) && s.Contains("free", StringComparison.OrdinalIgnoreCase);

    /// <summary>Looks up the human-readable model name from the listing, or <c>null</c> if not present.</summary>
    private static string? GetModelName(ProviderListing listing, ModelRef model)
    {
        var provider = listing.List.All.FirstOrDefault(p => string.Equals(p.Id, model.ProviderId, StringComparison.Ordinal));
        if (provider is null)
        {
            return null;
        }
        return provider.Models.TryGetValue(model.ModelId, out var m) ? m.Name : null;
    }

    /// <summary>
    /// Logs a warning when the resolved <paramref name="model"/> isn't in
    /// the server's connected-provider inventory, so a 500 from the server
    /// doesn't come as a surprise. No-op when the inventory is unavailable.
    /// Distinguishes three failure modes: model not on a connected provider,
    /// provider not connected, and provider not configured on the server.
    /// </summary>
    private void WarnIfModelUnavailable(ModelRef model, ProviderListing? listing)
    {
        if (listing is null || listing.AvailableModels.Count == 0)
        {
            return;
        }

        // Model is on a connected provider — everything is fine.
        if (listing.AvailableModels.TryGetValue(model.ProviderId, out var models) && models.Contains(model.ModelId))
        {
            return;
        }

        var connectedKeys = listing.AvailableModels.Keys;

        // Provider is on a connected provider but doesn't have this model.
        if (models is not null)
        {
            _logger.LogWarning(
                "Requested model {Provider}/{Model} is not in the server's model list. " +
                "Available models for provider {Provider}: {Models}. " +
                "The prompt will likely fail with a 500 until the model is configured or you pick one with --model.",
                model.ProviderId, model.ModelId, model.ProviderId, models);
            return;
        }

        // Provider exists on the server but is not currently connected.
        var providerInFullInventory = listing.List.All
            .Any(p => string.Equals(p.Id, model.ProviderId, StringComparison.Ordinal));
        if (providerInFullInventory)
        {
            _logger.LogWarning(
                "Requested provider {Provider} is configured on the server but is not currently connected. " +
                "Connected providers: {Providers}. " +
                "The prompt will likely fail with a 500 until {Provider} is connected or you pick a connected provider with --model.",
                model.ProviderId, connectedKeys, model.ProviderId);
            return;
        }

        // Provider doesn't exist on the server at all.
        _logger.LogWarning(
            "Requested provider {Provider} is not configured on the server. " +
            "Connected providers: {Providers}. " +
            "The prompt will likely fail with a 500 until the provider is configured or you pick one with --model.",
            model.ProviderId, connectedKeys);
    }

    /// <summary>Pair of the raw provider list and the flattened model-id map used for lookups.</summary>
    private sealed record ProviderListing(
        ProviderListResponse List,
        IReadOnlyDictionary<string, IReadOnlySet<string>> AvailableModels);
}
