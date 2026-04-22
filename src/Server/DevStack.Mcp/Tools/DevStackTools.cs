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
public class DevStackTools
{
    private readonly ILogger<DevStackTools> _logger;
    private readonly DevStackDbContext _dbContext;

    public DevStackTools(ILogger<DevStackTools> logger, DevStackDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    #region Project Tools

    [McpServerTool(Name = "devstack_getProjects"), Description("Read all projects from DevStack. Returns project name, id, and repository.")]
    public async Task<string> GetProjects()
    {
        var projects = await _dbContext.Projects.Select(p => new { p.Name, p.Id, p.Repository }).ToListAsync();
        return JsonSerializer.Serialize(projects);
    }

    [McpServerTool(Name = "devstack_getProjectById"), Description("Read a project by its ID. Returns project name and repository.")]
    public async Task<string> GetProjectById([Description("The project ID")][DefaultValue(null)] Guid? id)
    {
        if (id == null)
        {
            return JsonSerializer.Serialize(new { error = "Project ID is required" });
        }

        var project = await _dbContext.Projects.Where(p => p.Id == id.Value).Select(p => new { p.Name, p.Repository }).FirstOrDefaultAsync();
        if (project == null)
        {
            return JsonSerializer.Serialize(new { error = "Project not found" });
        }

        return JsonSerializer.Serialize(project);
    }

    #endregion

    #region Deliverable Tools

    [McpServerTool(Name = "devstack_getDeliverable"), Description("Read a deliverable by its ID.")]
    public async Task<string> GetDeliverable([Description("The deliverable ID")] Guid id)
    {
        var deliverable = await _dbContext.Deliverables.FindAsync([id]);
        if (deliverable == null)
            return JsonSerializer.Serialize(new { error = "Deliverable not found" });

        return JsonSerializer.Serialize(new { id = deliverable.Id.ToString(), title = deliverable.Title, description = deliverable.Description, type = deliverable.Type.ToString(), status = deliverable.Status.ToString() });
    }

    [McpServerTool(Name = "devstack_createDeliverable"), Description("Create a new deliverable (Feature) in DevStack. New deliverables are created in Ready state.")]
    public async Task<string> CreateDeliverable(
        [Description("The project ID")][DefaultValue(null)] Guid? projectId,
        [Description("The deliverable title")] string title,
        [Description("The deliverable description")][DefaultValue(null)] string? description,
        [Description("The acceptance criteria")][DefaultValue(null)] string? acceptanceCriteria,
        [Description("The execution plan")][DefaultValue(null)] string? executionPlan,
        [Description("The security impact assessment")][DefaultValue(null)] string? securityImpact,
        [Description("The performance impact assessment")][DefaultValue(null)] string? performanceImpact,
        [Description("The test plan")][DefaultValue(null)] string? testPlan,
        [Description("The deployment plan")][DefaultValue(null)] string? deploymentPlan)
    {
        try
        {
            var deliverable = new Deliverable
            {
                ProjectId = projectId ?? Guid.Empty,
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
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created deliverable with ID: {Id}", deliverable.Id);
            return JsonSerializer.Serialize(new { id = deliverable.Id.ToString(), type = "Feature", status = "Ready" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deliverable");
            throw;
        }
    }

    [McpServerTool(Name = "devstack_updateDeliverable"), Description("Modify an existing deliverable in DevStack.")]
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
        [Description("The updated blocking issues")][DefaultValue(null)] string? blocking)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id]);
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

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated deliverable with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), updated = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating deliverable: {Id}", id);
            throw;
        }
    }

    [McpServerTool(Name = "devstack_transitionDeliverableStatus"), Description("Change the state of a deliverable in DevStack.")]
    public async Task<string> TransitionDeliverableStatus(
        [Description("The deliverable ID")] Guid id,
        [Description("The target status")] DeliverableStatus targetStatus,
        [Description("The actor performing the transition")] string actor)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id]);
            if (deliverable == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            var service = new DeliverableStatusTransitionService();
            var result = service.Transition(deliverable, targetStatus, actor);

            if (!result.IsSuccess)
                return JsonSerializer.Serialize(new { error = result.Errors[0] });

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Transitioned deliverable {Id} to {Status} by {Actor}", id, targetStatus, actor);
            return JsonSerializer.Serialize(new { id = id.ToString(), status = targetStatus.ToString(), actor });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning deliverable status: {Id}", id);
            throw;
        }
    }

    #endregion

    #region Agent Task Tools

    [McpServerTool(Name = "devstack_getTask"), Description("Read an agent task by its ID.")]
    public async Task<string> GetTask([Description("The agent task ID")] Guid id)
    {
        var agentTask = await _dbContext.AgentTasks.FindAsync([id]);
        if (agentTask == null)
            return JsonSerializer.Serialize(new { error = "AgentTask not found" });

        return JsonSerializer.Serialize(new { id = agentTask.Id.ToString(), title = agentTask.Title, result = agentTask.Result, complexityRating = agentTask.ComplexityRating, status = agentTask.Status.ToString() });
    }

    [McpServerTool(Name = "devstack_createAgentTask"), Description("Create a new agent task in DevStack. New tasks are created in Ready state.")]
    public async Task<string> CreateAgentTask(
        [Description("The project ID")][DefaultValue(null)] Guid? projectId,
        [Description("The deliverable/feature ID")][DefaultValue(null)] Guid? itemId,
        [Description("The task title")] string title,
        [Description("The task status")][DefaultValue(null)] AgentTaskStatus? status,
        [Description("The task description")][DefaultValue(null)] string? description,
        [Description("The deliverable description")][DefaultValue(null)] string? deliverableDescription,
        [Description("The complexity rating (1-10)")] int complexityRating = 5)
    {
        try
        {
            var deliverableEntity = await _dbContext.Deliverables.FindAsync([itemId]);
            if (deliverableEntity == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            var agentTask = new AgentTask
            {
                ProjectId = projectId ?? Guid.Empty,
                DeliverableId = itemId ?? Guid.Empty,
                Title = title,
                Description = description ?? string.Empty,
                ComplexityRating = complexityRating,
                Status = status ?? AgentTaskStatus.Ready
            };

            _dbContext.AgentTasks.Add(agentTask);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created agent task with ID: {Id}", agentTask.Id);
            return JsonSerializer.Serialize(new { id = agentTask.Id.ToString(), status = "Ready" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent task");
            throw;
        }
    }

    [McpServerTool(Name = "devstack_updateAgentTask"), Description("Modify an existing agent task in DevStack.")]
    public async Task<string> UpdateAgentTask(
        [Description("The agent task ID")] Guid id,
        [Description("The updated status")][DefaultValue(null)] AgentTaskStatus? status,
        [Description("The result")][DefaultValue(null)] string? result,
        [Description("The errors")][DefaultValue(null)] string? errors,
        [Description("The commit hash")][DefaultValue(null)] string? commitHash,
        [Description("The agent")][DefaultValue(null)] string? agent)
    {
        try
        {
            var agentTask = await _dbContext.AgentTasks.FindAsync([id]);
            if (agentTask == null)
                return JsonSerializer.Serialize(new { error = "AgentTask not found" });

            if (status is not null) agentTask.Status = status.Value;
            if (result is not null) agentTask.Result = result;
            if (errors is not null) agentTask.Errors = errors;
            if (commitHash is not null) agentTask.CommitHash = commitHash;
            if (agent is not null) agentTask.Agent = agent;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated agent task with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), updated = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent task: {Id}", id);
            throw;
        }
    }

    [McpServerTool(Name = "devstack_transitionAgentTaskStatus"), Description("Change the state of an agent task in DevStack.")]
    public async Task<string> TransitionAgentTaskStatus(
        [Description("The agent task ID")] Guid id,
        [Description("The target status")] AgentTaskStatus targetStatus,
        [Description("The actor performing the transition")] string actor)
    {
        try
        {
            var agentTask = await _dbContext.AgentTasks.FindAsync([id]);
            if (agentTask == null)
                return JsonSerializer.Serialize(new { error = "AgentTask not found" });

            var service = new AgentTaskStatusTransitionService();
            var result = service.Transition(agentTask, targetStatus, actor);

            if (!result.IsSuccess)
                return JsonSerializer.Serialize(new { error = result.Errors[0] });

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Transitioned agent task {Id} to {Status} by {Actor}", id, targetStatus, actor);
            return JsonSerializer.Serialize(new { id = id.ToString(), status = targetStatus.ToString(), actor });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning agent task status: {Id}", id);
            throw;
        }
    }

    #endregion
}
