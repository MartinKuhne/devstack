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

    [McpServerTool(Name = "get_deliverable"), Description("Read a deliverable by its ID. Returns all fields including title, description, acceptance criteria, and status. Usage hint: Provide a valid deliverable ID.")]
    public async Task<string> GetDeliverable([Description("The deliverable ID")] Guid id, CancellationToken ct = default)
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

    [McpServerTool(Name = "get_next_deliverable"), Description("Find the next deliverable in Implement status for a project. Provide either a repository URL or a project ID.")]
    public async Task<string> GetNextDeliverable(
        [Description("The repository URL")][DefaultValue(null)] string? repositoryUrl,
        [Description("The project ID")][DefaultValue(null)] Guid? projectId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl) && (projectId == null || projectId == Guid.Empty))
            throw new McpProtocolException("Either repositoryUrl or projectId must be provided", McpErrorCode.InvalidParams);

        Project? project;
        if (projectId is not null && projectId != Guid.Empty)
        {
            project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId.Value, ct);
        }
        else
        {
            project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Repository == repositoryUrl, ct);
        }

        if (project == null)
            return ToolResponse.Error("Project not found");

        var deliverable = await _dbContext.Deliverables
            .Where(d => d.ProjectId == project.Id && d.Status == DeliverableStatus.Implement)
            .OrderBy(d => d.Id)
            .FirstOrDefaultAsync(ct);

        if (deliverable == null)
            return ToolResponse.Error("No deliverable found in Implement status for this project");

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

        return ToolResponse.Success("Next Deliverable", data);
    }

    [McpServerTool(Name = "create_deliverable"), Description("Create a new deliverable (Feature) in DevStack. New deliverables are created in Ready state. Usage hint: ProjectId must reference an existing project. Title and description are required fields.")]
    public async Task<string> CreateDeliverable(
        [Description("The project ID")][DefaultValue(null)] Guid? projectId,
        [Description("The deliverable title")] string title,
        [Description("The deliverable description")][DefaultValue(null)] string? description,
        [Description("The design document")][DefaultValue(null)] string? design,
        [Description("The acceptance criteria")][DefaultValue(null)] string? acceptanceCriteria,
        [Description("The execution plan")][DefaultValue(null)] string? executionPlan,
        [Description("The security impact assessment")][DefaultValue(null)] string? securityImpact,
        [Description("The performance impact assessment")][DefaultValue(null)] string? performanceImpact,
        [Description("The test plan")][DefaultValue(null)] string? testPlan,
        [Description("The deployment plan")][DefaultValue(null)] string? deploymentPlan,
        CancellationToken ct = default)
    {
        if (projectId == null || projectId == Guid.Empty)
        {
            throw new McpProtocolException("Project ID is required", McpErrorCode.InvalidParams);
        }

        var id = await _createDeliverableHandler.Handle(
            new CreateDeliverableCommand(
                projectId.Value,
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
        return ToolResponse.Success("Deliverable Created",
            new CreateDeliverableResponse(id.ToString(), projectId.Value.ToString(), "Feature", "Ready"),
            "Use the returned ID for subsequent get_deliverable, update_deliverable, or update_deliverable_status calls.");
    }

    [McpServerTool(Name = "update_deliverable"), Description("Modify an existing deliverable in DevStack. Only non-null fields are updated. Usage hint: Provide the deliverable ID and only the fields you want to change.")]
    public async Task<string> UpdateDeliverable(
        [Description("The deliverable ID")] Guid id,
        [Description("The updated description")][DefaultValue(null)] string? description,
        [Description("The updated design document")][DefaultValue(null)] string? design,
        [Description("The updated acceptance criteria")][DefaultValue(null)] string? acceptanceCriteria,
        [Description("The updated execution plan")][DefaultValue(null)] string? executionPlan,
        [Description("The updated security impact assessment")][DefaultValue(null)] string? securityImpact,
        [Description("The updated performance impact assessment")][DefaultValue(null)] string? performanceImpact,
        [Description("The updated test plan")][DefaultValue(null)] string? testPlan,
        [Description("The updated deployment plan")][DefaultValue(null)] string? deploymentPlan,
        [Description("The updated agent feedback")][DefaultValue(null)] string? agentFeedback,
        [Description("The updated blocking issues")][DefaultValue(null)] string? blocking,
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
            "Use get_deliverable to verify the changes.");
    }

    [McpServerTool(Name = "update_deliverable_status"), Description("Change the state of a deliverable in DevStack. Valid transitions are enforced by the state machine. Usage hint: Provide valid target status such as InProgress, Done, Failed, Rejected, or NeedsReview.")]
    public async Task<string> TransitionDeliverableStatus(
        [Description("The deliverable ID")] Guid id,
        [Description("The target status")] DeliverableStatus targetStatus,
        [Description("The actor performing the transition")] string actor,
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
