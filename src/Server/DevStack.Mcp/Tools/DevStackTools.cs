using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Defects;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Tasks;
using DevStack.Infrastructure.Persistence;
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
    public async Task<string> ReadProjects(
        [Description("Maximum number of projects to return")][DefaultValue(50)] int first,
        [Description("Number of projects to skip for pagination")][DefaultValue(0)] int skip,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.Projects.AsQueryable();
            var totalCount = query.Count();
            query = query.OrderBy(p => p.CreatedAt);
            if (skip > 0)
            {
                query = query.Skip(skip);
            }
            var projects = query.Take(first).ToList();

            var result = projects.Select(p => new
            {
                id = p.Id.ToString(),
                name = p.Name,
                repository = p.GithubUrl?.ToString() ?? string.Empty
            });

            var json = JsonSerializer.Serialize(result);
            _logger.LogInformation("Read {Count} projects", result.Count());
            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading projects");
            throw;
        }
    }

    [McpServerTool, Description("Get a single project by ID. Returns project name and repository.")]
    public async Task<string> GetProjectById([Description("The project ID")] Guid id)
    {
        try
        {
            var project = await _dbContext.Projects.FindAsync([id]);
            if (project == null)
            {
                return JsonSerializer.Serialize(new { error = $"Project with ID {id} not found." });
            }

            var result = new
            {
                id = project.Id.ToString(),
                name = project.Name,
                repository = project.GithubUrl?.ToString() ?? string.Empty
            };

            _logger.LogInformation("Get project by ID: {Id}", id);
            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project by ID: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Create a new deliverable (Feature) in DevStack. New deliverables are created in READY state.")]
    public async Task<string> CreateDeliverable(
        [Description("The project ID")][DefaultValue(null)] Guid? projectId,
        [Description("The deliverable title")] string title,
        [Description("The deliverable description")][DefaultValue(null)] string? description)
    {
        try
        {
            var handler = new CreateFeatureHandler(_dbContext);
            var command = new CreateFeatureCommand(
                projectId ?? Guid.Empty,
                title,
                description,
                null, null, null, null, null, null, null,
                FeatureStatus.Ready,
                null);

           var itemId = await handler.Handle(command, CancellationToken.None);
            var result = new { id = itemId.ToString() };

            _logger.LogInformation("Created deliverable with ID: {Id}", itemId);
            return JsonSerializer.Serialize(result);
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
            var handler = new UpdateFeatureHandler(_dbContext);
            var command = new UpdateFeatureCommand(
                id, title, description, null, null, null, null, null, null, null, null);

            await handler.Handle(command, CancellationToken.None);
            var result = new { id = id.ToString(), updated = true };

            _logger.LogInformation("Updated deliverable with ID: {Id}", id);
            return JsonSerializer.Serialize(result);
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
        [Description("The target status")] FeatureStatus targetStatus,
        [Description("The actor performing the transition")] string actor)
    {
        try
        {
            var handler = new TransitionFeatureStatusHandler(_dbContext, new ItemStatusTransitionService());
            var command = new TransitionFeatureStatusCommand(id, targetStatus, actor);

            await handler.Handle(command, CancellationToken.None);
            var result = new { id = id.ToString(), status = targetStatus.ToString(), actor };

            _logger.LogInformation("Transitioned deliverable {Id} to {Status} by {Actor}", id, targetStatus, actor);
            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning deliverable status: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Create a new agent task in DevStack. New tasks are created in READY state.")]
    public async Task<string> CreateAgentTask(
        [Description("The project ID")] Guid projectId,
        [Description("The task title")] string title,
        [Description("The task deliverable description")][DefaultValue(null)] string? deliverable,
        [Description("The task description")][DefaultValue(null)] string? description)
    {
        try
        {
            var handler = new CreateTaskHandler(_dbContext);
            var command = new CreateTaskCommand(
                projectId, title, description, deliverable, null, null, null, null, 5, FeatureStatus.Ready, null);

            var itemId = await handler.Handle(command, CancellationToken.None);
            var result = new { id = itemId.ToString() };

            _logger.LogInformation("Created agent task with ID: {Id}", itemId);
            return JsonSerializer.Serialize(result);
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
        [Description("The updated deliverable")][DefaultValue(null)] string? deliverable)
    {
        try
        {
            var handler = new UpdateTaskHandler(_dbContext);
            var command = new UpdateTaskCommand(
                id, title, null, deliverable, null, null, null, null, null);

            await handler.Handle(command, CancellationToken.None);
            var result = new { id = id.ToString(), updated = true };

            _logger.LogInformation("Updated agent task with ID: {Id}", id);
            return JsonSerializer.Serialize(result);
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
        [Description("The target status")] FeatureStatus targetStatus,
        [Description("The actor performing the transition")] string actor)
    {
        try
        {
            var handler = new TransitionTaskStatusHandler(_dbContext);
            var command = new TransitionTaskStatusCommand(id, targetStatus, actor);

            await handler.Handle(command, CancellationToken.None);
            var result = new { id = id.ToString(), status = targetStatus.ToString(), actor };

            _logger.LogInformation("Transitioned agent task {Id} to {Status} by {Actor}", id, targetStatus, actor);
            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning agent task status: {Id}", id);
            throw;
        }
    }
}
