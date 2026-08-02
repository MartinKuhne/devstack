using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace DevStack.Agent;

/// <summary>
/// Output of <see cref="PlanDeliverableLister"/>. Carries the matched
/// DevStack project alongside the list of PLAN-status deliverables so
/// the caller can print a coherent "for project X, here is the PLAN
/// queue" report without having to re-query.
/// </summary>
public sealed record PlanDeliverableReport(
    ProjectSummary Project,
    IReadOnlyList<DeliverableSummary> PlanDeliverables);

/// <summary>
/// End-to-end orchestrator for the <c>--show-plan</c> flow. Given a
/// <see cref="RepositoryContext"/>, resolves the matching DevStack
/// project (by canonical repository URL) and lists the
/// <c>PLAN</c>-status deliverables for it. Throws when the project
/// isn't registered on the DevStack side so the caller can surface a
/// clear "no project matches this repo" error.
/// </summary>
public sealed class PlanDeliverableLister
{
    private readonly DevStackProjectClient _projects;
    private readonly ILogger<PlanDeliverableLister> _logger;

    /// <summary>Builds the orchestrator with a project client and a logger.</summary>
    public PlanDeliverableLister(DevStackProjectClient projects, ILogger<PlanDeliverableLister> logger)
    {
        _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Looks up the project for <paramref name="context"/>'s canonical
    /// remote URL, then lists its PLAN-status deliverables.
    /// </summary>
    public async Task<PlanDeliverableReport> ListAsync(
        RepositoryContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var project = await _projects
            .FindProjectByRepositoryAsync(context.CanonicalRemoteUrl, cancellationToken)
            .ConfigureAwait(false);

        if (project is null)
        {
            throw new InvalidOperationException(
                $"No DevStack project is registered for repository '{context.CanonicalRemoteUrl}'. " +
                "Register the project on the DevStack side or point --repositoryRoot at a checkout that matches an existing project.");
        }

        _logger.LogInformation(
            "Resolved DevStack project {ProjectId} ({Name}) for repository '{Repository}'.",
            project.Id, project.Name, project.Repository);

        var deliverables = await _projects
            .ListPlanDeliverablesAsync(project.Id, cancellationToken)
            .ConfigureAwait(false);

        return new PlanDeliverableReport(project, deliverables);
    }
}
