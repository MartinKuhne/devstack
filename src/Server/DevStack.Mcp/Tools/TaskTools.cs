using DevStack.Application.AgentTasks;
using DevStack.Application.AgentTasks.Commands;
using DevStack.Application.AgentTasks.Queries;
using DevStack.Domain.Services;
using DevStack.Mcp.Dto;

using ModelContextProtocol;

namespace DevStack.Mcp.Tools;

[McpServerToolType]
public class TaskTools
{
    private readonly ILogger<TaskTools> _logger;
    private readonly DevStackDbContext _dbContext;
    private readonly ICommandHandler<Guid, CreateAgentTaskCommand> _createAgentTaskHandler;
    private readonly ICommandHandler<UpdateAgentTaskCommand> _updateAgentTaskHandler;
    private readonly ICommandHandler<UpdateAgentTaskStatusCommand> _updateAgentTaskStatusHandler;
    private readonly ICommandHandler<AgentTask, GetAgentTaskByIdQuery> _getAgentTaskByIdHandler;

    public TaskTools(
        ILogger<TaskTools> logger,
        DevStackDbContext dbContext,
        ICommandHandler<Guid, CreateAgentTaskCommand> createAgentTaskHandler,
        ICommandHandler<UpdateAgentTaskCommand> updateAgentTaskHandler,
        ICommandHandler<UpdateAgentTaskStatusCommand> updateAgentTaskStatusHandler,
        ICommandHandler<AgentTask, GetAgentTaskByIdQuery> getAgentTaskByIdHandler)
    {
        _logger = logger;
        _dbContext = dbContext;
        _createAgentTaskHandler = createAgentTaskHandler;
        _updateAgentTaskHandler = updateAgentTaskHandler;
        _updateAgentTaskStatusHandler = updateAgentTaskStatusHandler;
        _getAgentTaskByIdHandler = getAgentTaskByIdHandler;
    }

    [McpServerTool(Name = "get_task"), Description(Descriptions.TaskTools.GetTask)]
    public async Task<string> GetTask([Description(Descriptions.TaskTools.Id)] Guid id, CancellationToken ct = default)
    {
        // BUG-02 — the not-found guard below is unreachable, so get_task reports errors differently from its
        // sibling get_deliverable for the identical condition.
        // Category:        error-contract (inconsistent client-visible contract)
        // Trigger path:    client calls get_task with an unknown id
        //                  -> the GetAgentTaskByIdHandler.Handle(...) call below is awaited
        //                  -> AgentTaskHandlers.cs GetAgentTaskByIdHandler.Handle FindAsync returns null and the
        //                     handler throws InvalidOperationException("AgentTask with ID ... not found.")
        //                  -> the exception unwinds past the null check below, so the McpProtocolException is
        //                     never constructed and the null check is dead code
        //                  -> the MCP SDK catches the generic exception and returns a CallToolResult with
        //                     IsError = true and the text "An error occurred invoking 'get_task'."
        // Why this is a real bug: the declared return type is Task<AgentTask> (non-nullable), so the compiler
        //                  already proves the null branch cannot be taken; the guard expresses an intent
        //                  (InvalidParams for a bad id) that the implementation never delivers.
        // Why this is NOT a false positive: the two outcomes are observably different client paths - a tool that
        //                  throws McpProtocolException makes the client re-throw with the original message, while
        //                  any other exception yields an IsError result with a generic message. get_deliverable
        //                  returns a clean {"error":"Deliverable not found"} and get_project raises a protocol
        //                  error naming the id, so three sibling lookups disagree. The integration suite codifies
        //                  the current split (get_task under "tool execution errors", get_project under "protocol
        //                  errors"), which documents the inconsistency rather than resolving it.
        // Concrete consequence: an agent cannot distinguish "I passed a bad id" from "the server is broken", so
        //                  it retries a request that can never succeed; the raw .NET exception text also reaches
        //                  the client.
        // Confidence:      Confirmed.
        // Fix direction:   make GetAgentTaskByIdHandler return AgentTask? (mirroring GetDeliverableByIdHandler)
        //                  so the null guard below executes, or delete the dead guard and document the tool-error
        //                  contract as intentional.
        // Pinned by:       DevStack.Tests.Integration.MCP Features/ToolErrorPaths.feature (get_task unknown id
        //                  is listed under "Failed entity lookups ... surfaced as tool execution errors").
        var agentTask = await _getAgentTaskByIdHandler.Handle(new GetAgentTaskByIdQuery(id), ct);
        if (agentTask == null)
            throw new McpProtocolException($"AgentTask with ID {id} not found", McpErrorCode.InvalidParams);

        var data = new GetAgentTaskResponse(
            agentTask.Id.ToString(),
            agentTask.ProjectId.ToString(),
            agentTask.Title,
            agentTask.Status.ToString(),
            agentTask.Description,
            agentTask.Result,
            agentTask.Errors,
            agentTask.CommitHash,
            agentTask.Agent);

        return ToolResponse.Success("Agent Task", data);
    }

    [McpServerTool(Name = "get_next_task"), Description(Descriptions.TaskTools.GetNextTask)]
    public async Task<string> GetNextTask(
        [Description(Descriptions.TaskTools.RepositoryUrl)][DefaultValue(null)] string? repositoryUrl,
        [Description(Descriptions.TaskTools.ProjectId)][DefaultValue(null)] Guid? projectId,
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

        var deliverables = await _dbContext.Deliverables
            .Where(d => d.ProjectId == project.Id && d.Status == DeliverableStatus.Implement)
            .ToListAsync(ct);

        if (deliverables.Count == 0)
            return ToolResponse.Error("No deliverables found in Implement status for this project");

        var deliverableIds = deliverables.Select(d => d.Id).ToList();

        var tasks = await _dbContext.AgentTasks
            .Where(t => deliverableIds.Contains(t.DeliverableId))
            .ToListAsync(ct);

        var selectionResult = TaskSelectionService.SelectNextTask(deliverables, tasks);
        if (selectionResult == null)
            return ToolResponse.Error("No deliverables found in Implement status for this project");

        var (bestDeliverable, nextTask) = selectionResult.Value;

        // BUG-06 — this "all done" payload puts a DELIVERABLE id under the key "Id", in the shape of a task.
        // Category:        wrong-data (client-confusing payload shape)
        // Trigger path:    client calls get_next_task(projectId | repositoryUrl) for a project whose deliverables
        //                  are all in Implement and whose tasks are all terminal
        //                  -> :123 TaskSelectionService.SelectNextTask returns (bestDeliverable, nextTask: null)
        //                  -> :129-131 the success payload is an anonymous object with keys Message, Id, Title,
        //                     where Id/Title come from bestDeliverable
        //                  -> the task-found path (:133-144) returns GetAgentTaskResponse, whose keys are
        //                     Id, ProjectId, Title, Status, Description, Result, Errors, CommitHash, Agent
        //                  -> both are wrapped by ToolResponse.Success under the same "data" shape, so a client
        //                     that reads .Id cannot tell which entity it received
        // Why this is a real bug: the tool is documented as "Find the next task to work on"
        //                  (Descriptions.TaskTools.GetNextTask) and its usage hint tells the caller to use the
        //                  returned ID for get_task / update_task (Descriptions.TaskTools.CreateUsageHint). On
        //                  this branch the returned ID is not a
        //                  task id, so following the documented hint is guaranteed to fail.
        // Why this is NOT a false positive: the two payloads share the key names "Id" and "Title" but name
        //                  different entities, and the discriminator is only the human-readable title string
        //                  ("No Pending Tasks" vs "Next Task") plus the absence of Status - neither of which the
        //                  usage hint tells the client to check. The sibling get_next_deliverable
        //                  (DeliverableTools.GetNextDeliverable) uses GetNextDeliverableResponse, which also
        //                  exposes a bare
        //                  "Id", so "Id" is not a stable entity marker anywhere in this surface.
        // Concrete consequence: an agent that takes the Id and calls get_task(id) gets "AgentTask with ID ...
        //                  not found" (or, worse, an unrelated task if the ids ever collide), and cannot report
        //                  "nothing left to do" reliably.
        // Confidence:      Confirmed.
        // Fix direction:   give the payload unambiguous keys, e.g.
        //                  new { Message = "...", DeliverableId = bestDeliverable.Id, DeliverableTitle =
        //                  bestDeliverable.Title, TaskId = (string?)null } and say so in the usage hint.
        // Pinned by:       DevStack.Tests.Integration.MCP Features/ToolCoverageHappy.feature
        //                  "get_next_task reports all tasks are completed" and
        //                  Steps/ToolCoverageSteps.cs (the assertion that reads payload.Id on this branch).
        //                  That assertion fails once the key is renamed - update it with the fix.
        if (nextTask == null)
            return ToolResponse.Success("No Pending Tasks",
                new { Message = "All tasks are completed for the selected deliverable", bestDeliverable.Id, bestDeliverable.Title });

        var data = new GetAgentTaskResponse(
            nextTask.Id.ToString(),
            nextTask.ProjectId.ToString(),
            nextTask.Title,
            nextTask.Status.ToString(),
            nextTask.Description,
            nextTask.Result,
            nextTask.Errors,
            nextTask.CommitHash,
            nextTask.Agent);

        return ToolResponse.Success("Next Task", data);
    }

    [McpServerTool(Name = "create_task"), Description(Descriptions.TaskTools.CreateTask)]
    public async Task<string> CreateAgentTask(
        [Description(Descriptions.TaskTools.DeliverableId)] Guid deliverableId,
        [Description(Descriptions.TaskTools.Title)] string title,
        [Description(Descriptions.TaskTools.Description)][DefaultValue(null)] string? description,
        CancellationToken ct = default)
    {
        var deliverable = await _dbContext.Deliverables.FirstOrDefaultAsync(d => d.Id == deliverableId, ct);
        if (deliverable == null)
        {
            throw new McpProtocolException($"Deliverable with ID {deliverableId} not found", McpErrorCode.InvalidParams);
        }

        var id = await _createAgentTaskHandler.Handle(
            new CreateAgentTaskCommand(
                deliverable.ProjectId,
                deliverableId,
                title,
                description ?? string.Empty,
                5,
                null),
            ct);

        _logger.LogInformation("Created agent task with ID: {Id}", id);
        return ToolResponse.Success("Task Created",
            new CreateAgentTaskResponse(id.ToString(), "Ready"),
            Descriptions.TaskTools.CreateUsageHint);
    }

    [McpServerTool(Name = "update_task"), Description(Descriptions.TaskTools.UpdateTask)]
    public async Task<string> UpdateAgentTask(
        [Description(Descriptions.TaskTools.Id)] Guid id,
        [Description(Descriptions.TaskTools.Status)][DefaultValue(null)] AgentTaskStatus? status,
        [Description(Descriptions.TaskTools.Description)][DefaultValue(null)] string? description,
        [Description(Descriptions.TaskTools.Result)][DefaultValue(null)] string? result,
        [Description(Descriptions.TaskTools.Errors)][DefaultValue(null)] string? errors,
        [Description(Descriptions.TaskTools.CommitHash)][DefaultValue(null)] string? commitHash,
        [Description(Descriptions.TaskTools.Agent)][DefaultValue(null)] string? agent,
        CancellationToken ct = default)
    {
        await _updateAgentTaskHandler.Handle(
            new UpdateAgentTaskCommand(
                id,
                null,
                description,
                result,
                errors,
                commitHash,
                null,
                null,
                null,
                null,
                null,
                agent),
            ct);

        _logger.LogInformation("Updated agent task with ID: {Id}", id);
        return ToolResponse.Success("Task Updated",
            new UpdateAgentTaskResponse(id.ToString(), true),
            Descriptions.TaskTools.UpdateUsageHint);
    }

    [McpServerTool(Name = "update_task_status"), Description(Descriptions.TaskTools.UpdateTaskStatus)]
    public async Task<string> TransitionAgentTaskStatus(
        [Description(Descriptions.TaskTools.Id)] Guid id,
        [Description(Descriptions.TaskTools.TargetStatus)] AgentTaskStatus targetStatus,
        [Description(Descriptions.TaskTools.Actor)] string actor,
        CancellationToken ct = default)
    {
        await _updateAgentTaskStatusHandler.Handle(
            new UpdateAgentTaskStatusCommand(id, targetStatus, actor),
            ct);

        _logger.LogInformation("Transitioned agent task {Id} to {Status} by {Actor}", id, targetStatus, actor);
        return ToolResponse.Success("Task State Transitioned",
            new TransitionAgentTaskStatusResponse(id.ToString(), targetStatus.ToString(), actor));
    }
}
