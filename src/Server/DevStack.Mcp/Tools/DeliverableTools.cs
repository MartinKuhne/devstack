using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace DevStack.Mcp.Tools;

[McpServerToolType]
public class DeliverableTools
{
    private readonly ILogger<DeliverableTools> _logger;
    private readonly DevStackDbContext _dbContext;

    public DeliverableTools(ILogger<DeliverableTools> logger, DevStackDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [McpServerTool(Name = "get_deliverable"), Description("Read a deliverable by its ID. Returns all fields including title, description, acceptance criteria, and status. Usage hint: Provide a valid deliverable ID.")]
    public async Task<string> GetDeliverable([Description("The deliverable ID")] Guid id, CancellationToken ct = default)
    {
        var deliverable = await _dbContext.Deliverables.FindAsync([id], ct);
        if (deliverable == null)
            return JsonSerializer.Serialize(new { error = "Deliverable not found" });

        var data = new { id = deliverable.Id.ToString(), projectId = deliverable.ProjectId.ToString(), title = deliverable.Title, description = deliverable.Description, acceptanceCriteria = deliverable.AcceptanceCriteria, executionPlan = deliverable.ExecutionPlan, securityImpact = deliverable.SecurityImpact, performanceImpact = deliverable.PerformanceImpact, testPlan = deliverable.TestPlan, deploymentPlan = deliverable.DeploymentPlan, agentFeedback = deliverable.AgentFeedback, blocking = deliverable.Blocking };
        return $"## Deliverable\n\n```json\n{JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}\n```\n\n";
    }

   [McpServerTool(Name = "create_deliverable"), Description("Create a new deliverable (Feature) in DevStack. New deliverables are created in Ready state. Usage hint: ProjectId must reference an existing project. Title and description are required fields.")]
    public async Task<string> CreateDeliverable(
        [Description("The project ID")][DefaultValue(null)] Guid? projectId,
        [Description("The deliverable title")] string title,
        [Description("The deliverable description")][DefaultValue(null)] string? description,
        [Description("The acceptance criteria")][DefaultValue(null)] string? acceptanceCriteria,
        [Description("The execution plan")][DefaultValue(null)] string? executionPlan,
        [Description("The security impact assessment")][DefaultValue(null)] string? securityImpact,
        [Description("The performance impact assessment")][DefaultValue(null)] string? performanceImpact,
        [Description("The test plan")][DefaultValue(null)] string? testPlan,
        [Description("The deployment plan")][DefaultValue(null)] string? deploymentPlan,
        CancellationToken ct = default)
    {
        try
        {
            if (projectId == null || projectId == Guid.Empty)
            {
                throw new ArgumentException("Project ID is required");
            }

            var project = await _dbContext.Projects.AnyAsync(p => p.Id == projectId.Value, ct);
            if (!project)
            {
                throw new KeyNotFoundException($"Project with ID {projectId.Value} not found");
            }

            var deliverable = new Deliverable
            {
                ProjectId = projectId.Value,
                Title = title,
                Description = description,
                AcceptanceCriteria = acceptanceCriteria,
                ExecutionPlan = executionPlan,
                SecurityImpact = securityImpact,
                PerformanceImpact = performanceImpact,
                TestPlan = testPlan,
                DeploymentPlan = deploymentPlan,
                Type = DeliverableType.Feature,
                Status = DeliverableStatus.Ready
            };

            _dbContext.Deliverables.Add(deliverable);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Created deliverable with ID: {Id}", deliverable.Id);
            var result = new { id = deliverable.Id.ToString(), projectId = projectId.Value.ToString(), type = "Feature", status = "Ready" };
            return $"## Deliverable Created\n\n```json\n{JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}\n```\n\nUsage hint: Use the returned ID for subsequent get_deliverable, update_deliverable, or update_deliverable_state calls.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deliverable");
            throw;
        }
    }

    [McpServerTool(Name = "update_deliverable"), Description("Modify an existing deliverable in DevStack. Only non-null fields are updated. Usage hint: Provide the deliverable ID and only the fields you want to change.")]
    public async Task<string> UpdateDeliverable(
        [Description("The deliverable ID")] Guid id,
        [Description("The updated description")][DefaultValue(null)] string? description,
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
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id], ct);
            if (deliverable == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            if (description is not null) deliverable.Description = description;
            if (acceptanceCriteria is not null) deliverable.AcceptanceCriteria = acceptanceCriteria;
            if (executionPlan is not null) deliverable.ExecutionPlan = executionPlan;
            if (securityImpact is not null) deliverable.SecurityImpact = securityImpact;
            if (performanceImpact is not null) deliverable.PerformanceImpact = performanceImpact;
            if (testPlan is not null) deliverable.TestPlan = testPlan;
            if (deploymentPlan is not null) deliverable.DeploymentPlan = deploymentPlan;
            if (agentFeedback is not null) deliverable.AgentFeedback = agentFeedback;
            if (blocking is not null) deliverable.Blocking = blocking;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Updated deliverable with ID: {Id}", id);
            var result = new { id = id.ToString(), updated = true };
            return $"## Deliverable Updated\n\n```json\n{JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}\n```\n\nUsage hint: Use get_deliverable to verify the changes.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating deliverable: {Id}", id);
            throw;
        }
    }

    [McpServerTool(Name = "update_deliverable_state"), Description("Change the state of a deliverable in DevStack. Valid transitions are enforced by the state machine. Usage hint: Provide valid target status such as InProgress, Done, Failed, Rejected, or NeedsReview.")]
    public async Task<string> TransitionDeliverableStatus(
        [Description("The deliverable ID")] Guid id,
        [Description("The target status")] DeliverableStatus targetStatus,
        [Description("The actor performing the transition")] string actor,
        CancellationToken ct = default)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id], ct);
            if (deliverable == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            var service = new DeliverableStatusTransitionService();
            var result = service.Transition(deliverable, targetStatus, actor);

            if (!result.IsSuccess)
                return JsonSerializer.Serialize(new { error = result.Errors[0] });

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Transitioned deliverable {Id} to {Status} by {Actor}", id, targetStatus, actor);
            var response = new { id = id.ToString(), status = targetStatus.ToString(), actor };
            return $"## Deliverable State Transitioned\n\n```json\n{JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })}\n```\n\n";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning deliverable status: {Id}", id);
            throw;
        }
    }
}
