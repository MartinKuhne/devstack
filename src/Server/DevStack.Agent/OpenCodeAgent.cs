using DevStack.OpenCode.Client;
using DevStack.OpenCode.Models;

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
        var result = await _client.Session.PromptAsync(
            session.Id,
            new SessionPromptRequest
            {
                Model = resolvedModel,
                Parts = new[] { PartInput.Text(prompt) },
            },
            cancellationToken).ConfigureAwait(false);

        PrintReply(result);
        return session.Id;
    }

    private void PrintReply(SessionMessageView result)
    {
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
