using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Projects;
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

    [McpServerTool, Description("Read all projects from DevStack. Returns project name, id, and repository.")]
    public async Task<string> ReadProjects()
    {
        var projects = await _dbContext.Projects.ToListAsync();
        return JsonSerializer.Serialize(projects);
    }

    [McpServerTool, Description("Read a project by its ID.")]
    public async Task<string> ReadProjectById([Description("The project ID")][DefaultValue(null)] Guid? id)
    {
        if (id == null)
        {
            return JsonSerializer.Serialize(new { error = "Project ID is required" });
        }

        var project = await _dbContext.Projects.FindAsync([id.Value]);
        if (project == null)
        {
            return JsonSerializer.Serialize(new { error = "Project not found" });
        }

        return JsonSerializer.Serialize(project);
    }

    [McpServerTool, Description("Create a new deliverable (Feature) in DevStack. New deliverables are created in Planning state.")]
    public async Task<string> CreateDeliverable(
        [Description("The project ID")][DefaultValue(null)] Guid? projectId,
        [Description("The deliverable title")] string title,
        [Description("The deliverable description")][DefaultValue(null)] string? description)
    {
        try
        {
            var deliverable = new Deliverable
            {
                ProjectId = projectId ?? Guid.Empty,
                Title = title,
                Description = description,
                Type = DeliverableType.Feature,
                Status = DeliverableStatus.Planning
            };

            _dbContext.Deliverables.Add(deliverable);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created deliverable with ID: {Id}", deliverable.Id);
            return JsonSerializer.Serialize(new { id = deliverable.Id.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deliverable");
            throw;
        }
    }

    [McpServerTool, Description("Update an existing deliverable (Feature) in DevStack.")]
    public async Task<string> UpdateDeliverable(
        [Description("The deliverable ID")] Guid id,
        [Description("The updated title")][DefaultValue(null)] string? title,
        [Description("The updated description")][DefaultValue(null)] string? description)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id]);
            if (deliverable == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            if (title is not null) deliverable.Title = title;
            if (description is not null) deliverable.Description = description;

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

    [McpServerTool, Description("Transition a deliverable (Feature) status in DevStack.")]
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

    [McpServerTool, Description("Create a new agent task in DevStack. New tasks are created in Ready state.")]
    public async Task<string> CreateAgentTask(
        [Description("The project ID")] Guid projectId,
        [Description("The deliverable ID")] Guid deliverableId,
        [Description("The task title")] string title,
        [Description("The complexity rating (1-10)")] int complexityRating = 5)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([deliverableId]);
            if (deliverable == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            var agentTask = new AgentTask
            {
                ProjectId = projectId,
                DeliverableId = deliverableId,
                Title = title,
                ComplexityRating = complexityRating,
                Status = AgentTaskStatus.Ready
            };

            _dbContext.AgentTasks.Add(agentTask);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created agent task with ID: {Id}", agentTask.Id);
            return JsonSerializer.Serialize(new { id = agentTask.Id.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent task");
            throw;
        }
    }

    [McpServerTool, Description("Update an existing agent task in DevStack.")]
    public async Task<string> UpdateAgentTask(
        [Description("The agent task ID")] Guid id,
        [Description("The updated title")][DefaultValue(null)] string? title,
        [Description("The result")][DefaultValue(null)] string? result)
    {
        try
        {
            var agentTask = await _dbContext.AgentTasks.FindAsync([id]);
            if (agentTask == null)
                return JsonSerializer.Serialize(new { error = "AgentTask not found" });

            if (title is not null) agentTask.Title = title;
            if (result is not null) agentTask.Result = result;

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

    [McpServerTool, Description("Transition an agent task status in DevStack.")]
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
}
