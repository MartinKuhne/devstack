using DevStack.Application.Deliverables;
using DevStack.Application.Deliverables.Commands;
using DevStack.Application.Deliverables.Queries;
using DevStack.Mcp.Dto;

using ModelContextProtocol;

namespace DevStack.Mcp.Tools;

[McpServerToolType]
public class DeliverableTools
{
    private readonly ILogger<DeliverableTools> _logger;
    private readonly DevStackDbContext _dbContext;
    private readonly ICommandHandler<Guid, CreateDeliverableCommand> _createDeliverableHandler;
    private readonly ICommandHandler<UpdateDeliverableCommand> _updateDeliverableHandler;
    private readonly ICommandHandler<UpdateDeliverableStatusCommand> _updateDeliverableStatusHandler;
    private readonly ICommandHandler<Deliverable?, GetDeliverableByIdQuery> _getDeliverableByIdHandler;

    public DeliverableTools(
        ILogger<DeliverableTools> logger,
        DevStackDbContext dbContext,
        ICommandHandler<Guid, CreateDeliverableCommand> createDeliverableHandler,
        ICommandHandler<UpdateDeliverableCommand> updateDeliverableHandler,
        ICommandHandler<UpdateDeliverableStatusCommand> updateDeliverableStatusHandler,
        ICommandHandler<Deliverable?, GetDeliverableByIdQuery> getDeliverableByIdHandler)
    {
        _logger = logger;
        _dbContext = dbContext;
        _createDeliverableHandler = createDeliverableHandler;
        _updateDeliverableHandler = updateDeliverableHandler;
        _updateDeliverableStatusHandler = updateDeliverableStatusHandler;
        _getDeliverableByIdHandler = getDeliverableByIdHandler;
    }

    [McpServerTool(Name = "get_deliverable"), Description(Descriptions.DeliverableTools.GetDeliverable)]
    public async Task<string> GetDeliverable([Description(Descriptions.DeliverableTools.Id)] Guid id, CancellationToken ct = default)
    {
        var deliverable = await _getDeliverableByIdHandler.Handle(new GetDeliverableByIdQuery(id), ct);
        if (deliverable == null)
            return ToolResponse.Error("Deliverable not found");

        var data = new GetDeliverableResponse(
            deliverable.Id.ToString(),
            deliverable.ProjectId.ToString(),
            deliverable.Title,
            deliverable.Description,
            deliverable.Design,
            deliverable.AcceptanceCriteria,
            deliverable.ExecutionPlan,
            deliverable.SecurityImpact,
            deliverable.PerformanceImpact,
            deliverable.TestPlan,
            deliverable.DeploymentPlan,
            deliverable.AgentFeedback,
            deliverable.Blocking);

        return ToolResponse.Success("Deliverable", data);
    }

    [McpServerTool(Name = "get_next_deliverable"), Description(Descriptions.DeliverableTools.GetNextDeliverable)]
    public async Task<string> GetNextDeliverable(
        [Description(Descriptions.DeliverableTools.ProjectId)] Guid projectId,
        [Description(Descriptions.DeliverableTools.Status)] DeliverableStatus status,
        CancellationToken ct = default)
    {
        if (projectId == Guid.Empty)
            throw new McpProtocolException("ProjectId must be provided", McpErrorCode.InvalidParams);

        var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project == null)
            return ToolResponse.Error("Project not found");

        var deliverable = await _dbContext.Deliverables
            .Where(d => d.ProjectId == project.Id && d.Status == status)
            .OrderBy(d => d.Id)
            .FirstOrDefaultAsync(ct);

        if (deliverable == null)
            return ToolResponse.Error($"No deliverable found in {status} status for this project");

        var data = new GetNextDeliverableResponse(deliverable.Id.ToString());

        return ToolResponse.Success("Next Deliverable", data);
    }

    [McpServerTool(Name = "create_deliverable"), Description(Descriptions.DeliverableTools.CreateDeliverable)]
    public async Task<string> CreateDeliverable(
        [Description(Descriptions.DeliverableTools.ProjectId)] Guid projectId,
        [Description(Descriptions.DeliverableTools.Title)] string title,
        [Description(Descriptions.DeliverableTools.Description)][DefaultValue(null)] string? description,
        [Description(Descriptions.DeliverableTools.Design)][DefaultValue(null)] string? design,
        [Description(Descriptions.DeliverableTools.AcceptanceCriteria)][DefaultValue(null)] string? acceptanceCriteria,
        [Description(Descriptions.DeliverableTools.ExecutionPlan)][DefaultValue(null)] string? executionPlan,
        [Description(Descriptions.DeliverableTools.SecurityImpact)][DefaultValue(null)] string? securityImpact,
        [Description(Descriptions.DeliverableTools.PerformanceImpact)][DefaultValue(null)] string? performanceImpact,
        [Description(Descriptions.DeliverableTools.TestPlan)][DefaultValue(null)] string? testPlan,
        [Description(Descriptions.DeliverableTools.DeploymentPlan)][DefaultValue(null)] string? deploymentPlan,
        CancellationToken ct = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new McpProtocolException("Project ID is required", McpErrorCode.InvalidParams);
        }

        var id = await _createDeliverableHandler.Handle(
            new CreateDeliverableCommand(
                projectId,
                DeliverableType.Feature,
                title,
                description,
                acceptanceCriteria,
                executionPlan,
                securityImpact,
                performanceImpact,
                testPlan,
                deploymentPlan,
                Domain.Enums.DeliverableStatus.Draft,
                design),
            ct);

        _logger.LogInformation("Created deliverable with ID: {Id}", id);
        // BUG-03 — the response below reports a status that was never persisted and that does not exist.
        // Category:        wrong-data (client-visible)
        // Trigger path:    client calls create_deliverable(projectId, title)
        //                  -> :107-121 CreateDeliverableCommand is built with InitialStatus
        //                     = Domain.Enums.DeliverableStatus.Draft (the literal at :119)
        //                  -> CreateDeliverableHandler passes that status to the Deliverable constructor, so the
        //                     persisted row has Status = Draft
        //                  -> :125 the response is built from two string LITERALS, "Feature" and "Ready"; neither
        //                     value is read back from the created entity
        //                  -> ToolResponse.Success serialises them verbatim into the tool result
        // Why this is a real bug: "Ready" is not a member of DeliverableStatus at all
        //                  (DeliverableStatus.cs:4-16 declares Draft, Design, Plan, Implement, Merge, Deploy,
        //                  Test, Done, Failed, Rejected, NeedsReview). The tool therefore advertises a state the
        //                  domain cannot represent, and the real state (Draft) is different from the reported one.
        //                  An agent that trusts the payload and moves on to the next step of the workflow acts on a
        //                  deliverable it believes is ready but that is still a draft.
        // Why this is NOT a false positive: the mismatch is between two lines of the same method (:119 vs :125),
        //                  so no later code path can reconcile them - the handler is awaited and SaveChanges has
        //                  already committed Draft before the response is constructed. The sibling get_deliverable
        //                  path (:44-56) DOES read deliverable.Status, which shows the literal here is an
        //                  oversight rather than a deliberate contract.
        // Concrete consequence: a client that filters with get_next_deliverable(status: Ready) finds nothing, and
        //                  one that branches on the reported status takes the "ready" branch for a Draft row.
        //                  Descriptions.DeliverableTools.CreateDeliverable compounds it by documenting "The tool
        //                  sets new deliverables to Ready state", so the model is told the same falsehood.
        // Confidence:      Confirmed (both the persisted value and the enum membership were read in source).
        // Fix direction:   read the persisted entity (or the enum) instead of literals:
        //                  new CreateDeliverableResponse(id.ToString(), projectId.ToString(),
        //                      DeliverableType.Feature.ToString(), DeliverableStatus.Draft.ToString())
        //                  - or re-fetch the deliverable and project its real Type/Status - and correct
        //                  Descriptions.DeliverableTools.CreateDeliverable accordingly.
        // Pinned by:       NO test today. No scenario asserts the Type/Status fields of the create_deliverable
        //                  payload, so this defect is currently unobserved by the suite; it is recorded in
        //                  doc/planning/integration-test-coverage-ledger.md §6 as BUG-03. Add the assertion when
        //                  the fix lands so the corrected values are locked in.
        return ToolResponse.Success("Deliverable Created",
            new CreateDeliverableResponse(id.ToString(), projectId.ToString(), "Feature", "Ready"),
            Descriptions.DeliverableTools.CreateUsageHint);
    }

    [McpServerTool(Name = "update_deliverable"), Description(Descriptions.DeliverableTools.UpdateDeliverable)]
    public async Task<string> UpdateDeliverable(
        [Description(Descriptions.DeliverableTools.Id)] Guid id,
        [Description(Descriptions.DeliverableTools.Description)][DefaultValue(null)] string? description,
        [Description(Descriptions.DeliverableTools.Design)][DefaultValue(null)] string? design,
        [Description(Descriptions.DeliverableTools.AcceptanceCriteria)][DefaultValue(null)] string? acceptanceCriteria,
        [Description(Descriptions.DeliverableTools.ExecutionPlan)][DefaultValue(null)] string? executionPlan,
        [Description(Descriptions.DeliverableTools.SecurityImpact)][DefaultValue(null)] string? securityImpact,
        [Description(Descriptions.DeliverableTools.PerformanceImpact)][DefaultValue(null)] string? performanceImpact,
        [Description(Descriptions.DeliverableTools.TestPlan)][DefaultValue(null)] string? testPlan,
        [Description(Descriptions.DeliverableTools.DeploymentPlan)][DefaultValue(null)] string? deploymentPlan,
        [Description(Descriptions.DeliverableTools.AgentFeedback)][DefaultValue(null)] string? agentFeedback,
        [Description(Descriptions.DeliverableTools.Blocking)][DefaultValue(null)] string? blocking,
        CancellationToken ct = default)
    {
        await _updateDeliverableHandler.Handle(
            new UpdateDeliverableCommand(
                id,
                null,
                description,
                acceptanceCriteria,
                executionPlan,
                agentFeedback,
                securityImpact,
                performanceImpact,
                testPlan,
                deploymentPlan,
                blocking,
                design),
            ct);

        _logger.LogInformation("Updated deliverable with ID: {Id}", id);
        return ToolResponse.Success("Deliverable Updated",
            new UpdateDeliverableResponse(id.ToString(), true),
            Descriptions.DeliverableTools.UpdateUsageHint);
    }

    [McpServerTool(Name = "update_deliverable_status"), Description(Descriptions.DeliverableTools.UpdateDeliverableStatus)]
    public async Task<string> TransitionDeliverableStatus(
        [Description(Descriptions.DeliverableTools.Id)] Guid id,
        [Description(Descriptions.DeliverableTools.TargetStatus)] DeliverableStatus targetStatus,
        [Description(Descriptions.DeliverableTools.Actor)] string actor,
        CancellationToken ct = default)
    {
        await _updateDeliverableStatusHandler.Handle(
            new UpdateDeliverableStatusCommand(id, targetStatus, actor),
            ct);

        _logger.LogInformation("Transitioned deliverable {Id} to {Status} by {Actor}", id, targetStatus, actor);
        return ToolResponse.Success("Deliverable State Transitioned",
            new TransitionDeliverableStatusResponse(id.ToString(), targetStatus.ToString(), actor));
    }
}
