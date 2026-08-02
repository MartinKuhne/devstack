using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using DevStack.OpenCode.Models;

using Microsoft.Extensions.Logging;

namespace DevStack.Agent;

/// <summary>
/// Drives the "plan mode" workflow: for every <c>PLAN</c>-status
/// deliverable in the report, render the prompt template (substituting
/// <c>{{DeliverableId}}</c> with the deliverable's id) and execute it
/// through the OpenCode SDK. One OpenCode session is created per
/// deliverable so each plan is its own conversation; failures in one
/// deliverable are logged and the run continues with the next one,
/// so a single misbehaving deliverable does not sink the whole batch.
/// </summary>
public sealed class PlanExecutor
{
    /// <summary>The exact substitution token the prompt template uses.</summary>
    public const string DeliverableIdToken = "{{DeliverableId}}";

    private readonly OpenCodeAgent _openCodeAgent;
    private readonly ILogger<PlanExecutor> _logger;

    /// <summary>Builds the executor with the OpenCode agent and a logger.</summary>
    public PlanExecutor(OpenCodeAgent openCodeAgent, ILogger<PlanExecutor> logger)
    {
        _openCodeAgent = openCodeAgent ?? throw new ArgumentNullException(nameof(openCodeAgent));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the plan for every deliverable in
    /// <paramref name="report"/>. The prompt template is loaded from
    /// <paramref name="promptPath"/>; relative paths are resolved
    /// against <see cref="RepositoryContext.Worktree"/>. Returns a
    /// summary of which deliverables were processed and which
    /// failed.
    /// </summary>
    public async Task<PlanRunSummary> ExecuteAsync(
        PlanDeliverableReport report,
        RepositoryContext context,
        string promptPath,
        ModelRef? model = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(context);
        if (string.IsNullOrWhiteSpace(promptPath))
        {
            throw new ArgumentException("promptPath must be non-empty", nameof(promptPath));
        }

        var resolvedPath = ResolvePromptPath(promptPath, context.Worktree);
        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException(
                $"Plan prompt template not found at '{resolvedPath}'. " +
                "Pass --plan-prompt <path> to point at the template, or set DevStack:Plan:PromptPath in appsettings.json.",
                resolvedPath);
        }

        var template = await File.ReadAllTextAsync(resolvedPath, cancellationToken).ConfigureAwait(false);
        if (!template.Contains(DeliverableIdToken, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Plan prompt template '{Path}' does not contain the {Token} token; substitution will be a no-op.",
                resolvedPath, DeliverableIdToken);
        }

        _logger.LogInformation(
            "Executing plan mode for {Count} deliverable(s) using template '{Path}'.",
            report.PlanDeliverables.Count, resolvedPath);

        var processed = new List<Guid>(report.PlanDeliverables.Count);
        var failures = new Dictionary<Guid, string>();

        foreach (var deliverable in report.PlanDeliverables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rendered = template.Replace(DeliverableIdToken, deliverable.Id.ToString(), StringComparison.Ordinal);
            var title = $"Plan: {deliverable.Title}";

            Console.WriteLine();
            Console.WriteLine($"→ Planning {deliverable.Title} ({deliverable.Id})");
            Console.WriteLine($"  type:     {deliverable.Type}");
            Console.WriteLine($"  status:   {deliverable.Status}");

            try
            {
                var sessionId = await _openCodeAgent
                    .RunAsync(rendered, model: model, title: title, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                processed.Add(deliverable.Id);
                Console.WriteLine();
                Console.WriteLine($"✓ Done. sessionId={sessionId}");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // One bad deliverable does not sink the whole batch.
                // We log, record the failure, and continue.
                _logger.LogError(ex,
                    "Plan execution failed for deliverable {Id} ({Title}). Continuing with the next one.",
                    deliverable.Id, deliverable.Title);
                failures[deliverable.Id] = ex.Message;
                Console.Error.WriteLine($"error: planning {deliverable.Id} failed: {ex.Message}");
            }
        }

        return new PlanRunSummary(processed, failures);
    }

    /// <summary>
    /// Resolves a possibly-relative prompt path against the worktree.
    /// Absolute paths (and rooted paths like <c>C:\foo</c>) are
    /// returned as-is; relative paths are interpreted relative to
    /// <paramref name="worktreeRoot"/>.
    /// </summary>
    internal static string ResolvePromptPath(string promptPath, string worktreeRoot)
    {
        if (Path.IsPathRooted(promptPath))
        {
            return Path.GetFullPath(promptPath);
        }
        return Path.GetFullPath(Path.Combine(worktreeRoot, promptPath));
    }
}

/// <summary>
/// Per-run summary returned by <see cref="PlanExecutor"/>. Tracks
/// the deliverables that were processed and any that failed so the
/// caller can print a final tally and pick a non-zero exit code if
/// anything went wrong.
/// </summary>
public sealed record PlanRunSummary(
    IReadOnlyList<Guid> Processed,
    IReadOnlyDictionary<Guid, string> Failures)
{
    /// <summary>True when every deliverable was processed without error.</summary>
    public bool AllSucceeded => Failures.Count == 0;
}
