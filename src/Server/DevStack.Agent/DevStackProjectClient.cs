using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DevStack.Agent.GraphQL;

using Microsoft.Extensions.Logging;

using StrawberryShake;

namespace DevStack.Agent;

/// <summary>
/// Thin wrapper around the StrawberryShake-generated
/// <see cref="IDevStackClient"/>. Returns plain
/// <see cref="ProjectSummary"/> records so the rest of the agent never has
/// to know about the generated GraphQL types. Each method surfaces server
/// errors via <see cref="IOperationResult{T}.EnsureNoErrors"/> rather than
/// swallowing them.
/// </summary>
public sealed class DevStackProjectClient
{
    private readonly IDevStackClient _client;
    private readonly ILogger<DevStackProjectClient> _logger;

    /// <summary>Builds the wrapper around the generated client and a logger.</summary>
    public DevStackProjectClient(IDevStackClient client, ILogger<DevStackProjectClient> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Fetches up to <paramref name="first"/> projects from the DevStack
    /// GraphQL API. Throws when the server returns GraphQL errors.
    /// </summary>
    public async Task<IReadOnlyList<ProjectSummary>> ListProjectsAsync(
        int first = 50,
        CancellationToken cancellationToken = default)
    {
        if (first <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(first), first, "first must be > 0");
        }

        _logger.LogInformation("Fetching up to {First} projects from the DevStack GraphQL API…", first);
        var result = await _client.GetProjects.ExecuteAsync(first, cancellationToken).ConfigureAwait(false);
        result.EnsureNoErrors();

        var nodes = result.Data?.Projects?.Nodes ?? (IReadOnlyList<IGetProjects_Projects_Nodes>)Array.Empty<IGetProjects_Projects_Nodes>();
        return nodes.Select(n => new ProjectSummary(n.Id, n.Name, n.Description, n.Repository)).ToList();
    }

    /// <summary>
    /// Looks up a single project by id. Returns <c>null</c> when the server
    /// has no project with that id; throws on GraphQL errors (validation
    /// failures, network issues, etc.).
    /// </summary>
    public async Task<ProjectSummary?> GetProjectByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching project {Id} from the DevStack GraphQL API…", id);
        var result = await _client.GetProjectById.ExecuteAsync(id, cancellationToken).ConfigureAwait(false);
        result.EnsureNoErrors();

        var project = result.Data?.Project;
        return project is null
            ? null
            : new ProjectSummary(project.Id, project.Name, project.Description, project.Repository);
    }

    /// <summary>
    /// Resolves a project by its canonical repository URL (e.g.
    /// <c>https://github.com/owner/repo</c>). Returns <c>null</c> when no
    /// project has that repository registered; throws on GraphQL errors.
    /// </summary>
    public async Task<ProjectSummary?> FindProjectByRepositoryAsync(
        string repository,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(repository))
        {
            throw new ArgumentException("repository must be non-empty", nameof(repository));
        }

        _logger.LogInformation("Looking up the DevStack project with repository '{Repository}'…", repository);
        var result = await _client.GetProjectByRepository.ExecuteAsync(repository, cancellationToken).ConfigureAwait(false);
        result.EnsureNoErrors();

        var node = result.Data?.Projects?.Nodes is { Count: > 0 } nodes ? nodes[0] : null;
        return node is null
            ? null
            : new ProjectSummary(node.Id, node.Name, node.Description, node.Repository);
    }

    /// <summary>
    /// Lists the deliverables in <c>PLAN</c> status for the given project.
    /// Sorted by title server-side; throws on GraphQL errors. The status
    /// filter is hard-coded to <c>PLAN</c> because the only caller is the
    /// <c>--show-plan</c> flow — adding a status parameter would just
    /// invite mistakes.
    /// </summary>
    public async Task<IReadOnlyList<DeliverableSummary>> ListPlanDeliverablesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("projectId must be non-empty", nameof(projectId));
        }

        _logger.LogInformation("Listing PLAN-status deliverables for project {ProjectId}…", projectId);
        var result = await _client.GetPlanDeliverables.ExecuteAsync(projectId, cancellationToken).ConfigureAwait(false);
        result.EnsureNoErrors();

        var nodes = result.Data?.Deliverables?.Nodes ?? (IReadOnlyList<IGetPlanDeliverables_Deliverables_Nodes>)Array.Empty<IGetPlanDeliverables_Deliverables_Nodes>();
        return nodes.Select(n => new DeliverableSummary(
            n.Id,
            n.ProjectId,
            n.Type.ToString(),
            n.Title,
            n.Status.ToString(),
            n.Description)).ToList();
    }
}
