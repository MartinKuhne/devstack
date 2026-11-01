using DevStack.Application.AgentTasks;
using DevStack.Application.AgentTasks.Commands;
using DevStack.Application.AgentTasks.Queries;

namespace DevStack.Infrastructure.AgentTasks;

public class CreateAgentTaskHandler : ICommandHandler<Guid, CreateAgentTaskCommand>
{
    private readonly DevStackDbContext _dbContext;

    public CreateAgentTaskHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateAgentTaskCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ArgumentException("Title is required", nameof(command.Title));

        // DRIFT-09 — the create path caps the title at 200 characters while the entity and the column allow 300.
        // Category:        validation-bypass / inconsistent validation between create and update
        // Trigger path:    createAgentTask(input: { title: <201 chars> })
        //                  -> this guard throws ArgumentException("Title must be 200 characters or less")
        //                  -> updateAgentTask(input: { id, title: <the same 201 chars> }) goes through
        //                     AgentTask.UpdateMetadata (AgentTask.cs:109), which allows up to 300, and succeeds
        // Why this is a real bug: the authoritative limit is the column, AgentTaskConfiguration.cs:16
        //                  (HasMaxLength(300)), and the entity agrees with it (AgentTask.cs:69 and :109 both use
        //                  300). This handler invents a stricter rule, so the same value is rejected on create
        //                  and accepted on update for no documented reason.
        // Why this is NOT a false positive: the three limits were read side by side - 200 here, 300 in both
        //                  entity paths, 300 in the EF mapping and in the migration
        //                  (20260413165634_InitialCreate.cs:59, character varying(300)). The integration suite
        //                  creates a task with a title that the update path would accept and observes the create
        //                  being rejected, so the disagreement is observed rather than inferred.
        // Concrete consequence: callers cannot predict whether a title is legal; a client that round-trips a
        //                  task (read 250-char title, send it back on create) gets an error for data the
        //                  database already stores.
        // Confidence:      Confirmed.
        // Fix direction:   use one shared rule - ValidationRules.ValidateTaskTitle (ValidationRules.cs:21) or the
        //                  entity's own limit - in both handlers instead of a local constant.
        // Pinned by:       DevStack.Tests.Integration.GraphQL (the create/update title-boundary scenarios assert
        //                  the 200/300 split today; collapse them when the rules are unified).
        if (command.Title.Length > 200)
            throw new ArgumentException("Title must be 200 characters or less", nameof(command.Title));

        var deliverable = await _dbContext.Deliverables.FindAsync([command.DeliverableId], cancellationToken);
        if (deliverable == null)
            throw new InvalidOperationException($"Deliverable with ID {command.DeliverableId} not found.");

        // DRIFT-11 — the dependency is only checked for existence, never for scope.
        // Category:        invariant-violation (cross-aggregate reference accepted)
        // Trigger path:    createAgentTask(input: { deliverableId: A, dependsOnAgentTaskId: X }) where X belongs
        //                  to a different deliverable, or to a different project entirely
        //                  -> this block FindAsync's X and only checks that it is not null
        //                  -> no comparison of dependencyTask.DeliverableId (or ProjectId) against the new task's
        //                  -> the row is persisted with the cross-boundary dependency and the mutation returns the
        //                     task with dependsOnAgentTaskId pointing at it
        // Why this is a real bug: a task dependency is meaningful only inside the deliverable that schedules it -
        //                  get_next_task selects tasks by deliverable (:119-121) - so a dependency on a foreign
        //                  task is data the scheduler can never satisfy or explain.
        // Why this is NOT a false positive: the integration suite creates two deliverables in two projects,
        //                  links a task in one to a task in the other, and reads the link back: the server
        //                  accepts it. No orphan rows are produced, which is why nothing else complains - the
        //                  graph is simply meaningless. deleteAgentTask on the depended-on task leaves the
        //                  dependent row in place with dependsOnAgentTaskId cleared (AgentTaskConfiguration.cs:53
        //                  OnDelete SetNull), which is safe but silent: the ordering constraint disappears with
        //                  no signal to the client.
        // Concrete consequence: an agent that honours dependsOnAgentTaskId waits on a task that is not part of
        //                  the deliverable it is working on, and the ordering constraint is lost silently when
        //                  the depended-on task is deleted.
        // Confidence:      Confirmed.
        // Fix direction:   require dependencyTask.DeliverableId == command.DeliverableId (or at least the same
        //                  ProjectId) and reject otherwise; decide explicitly whether SetNull-on-delete should
        //                  instead block the delete. Product-owner decision recorded as DRIFT-11.
        // Pinned by:       DevStack.Tests.Integration.GraphQL Features/GraphQLReferentialIntegrity.feature
        //                  (cross-deliverable dependency accepted; dependency cleared on delete).
        if (command.DependsOnAgentTaskId.HasValue)
        {
            var dependencyTask = await _dbContext.AgentTasks.FindAsync([command.DependsOnAgentTaskId.Value], cancellationToken);
            if (dependencyTask == null)
                throw new InvalidOperationException($"Dependent AgentTask with ID {command.DependsOnAgentTaskId.Value} not found.");
        }

        // DRIFT-12 — command.ProjectId is ignored: the project is derived from the deliverable.
        // Category:        misleading-input (schema field with no effect)
        // Trigger path:    createAgentTask(input: { deliverableId: D, projectId: <any value, even a non-existent
        //                  id> }) -> this line takes projectId from deliverable.ProjectId and command.ProjectId is
        //                  never read anywhere in the handler -> the mutation succeeds and the persisted row
        //                  carries the deliverable's project
        // Why this is a real bug: src/schema.graphql publishes projectId on CreateAgentTaskInput, so callers are
        //                  invited to supply it and may reasonably use it to validate intent. A caller that
        //                  passes a contradicting project gets a successful mutation whose row belongs to a
        //                  different project than requested - the input is silently discarded rather than checked.
        // Why this is NOT a false positive: the derived value is visible in the persisted row, which the
        //                  integration suite reads back after sending a deliberately wrong projectId; the row
        //                  keeps the deliverable's project and no error is raised. The derived value is at least
        //                  self-consistent, so this is drift rather than corruption - hence a decision, not a fix.
        // Concrete consequence: a client that believes projectId is honoured (for example, one that scopes
        //                  permissions by it) writes tasks into a project it did not select.
        // Confidence:      Confirmed.
        // Fix direction:   remove projectId from CreateAgentTaskInput, or validate that it equals
        //                  deliverable.ProjectId and reject a mismatch. Product-owner decision recorded as DRIFT-12.
        // Pinned by:       DevStack.Tests.Integration.GraphQL Features/GraphQLReferentialIntegrity.feature
        //                  (contradicting projectId is accepted and the deliverable's project wins).
        var agentTask = new AgentTask(
            projectId: deliverable.ProjectId,
            deliverableId: command.DeliverableId,
            title: command.Title,
            description: command.Description,
            complexityRating: command.ComplexityRating,
            dependsOnAgentTaskId: command.DependsOnAgentTaskId,
            status: AgentTaskStatus.Ready);

        _dbContext.AgentTasks.Add(agentTask);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return agentTask.Id;
    }
}

public class UpdateAgentTaskHandler : ICommandHandler<UpdateAgentTaskCommand>
{
    private readonly DevStackDbContext _dbContext;

    public UpdateAgentTaskHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateAgentTaskCommand command, CancellationToken cancellationToken)
    {
        var agentTask = await _dbContext.AgentTasks.FindAsync([command.Id], cancellationToken);
        if (agentTask == null)
            throw new InvalidOperationException($"AgentTask with ID {command.Id} not found.");

        agentTask.UpdateMetadata(
            title: command.Title,
            description: command.Description,
            result: command.Result,
            errors: command.Errors,
            commitHash: command.CommitHash,
            complexityRating: command.ComplexityRating,
            promptTokens: command.PromptTokens,
            completionTokens: command.CompletionTokens,
            executionDurationInSeconds: command.ExecutionDurationInSeconds,
            agent: command.Agent);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class UpdateAgentTaskStatusHandler : ICommandHandler<UpdateAgentTaskStatusCommand>
{
    private readonly DevStackDbContext _dbContext;

    public UpdateAgentTaskStatusHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateAgentTaskStatusCommand command, CancellationToken cancellationToken)
    {
        var agentTask = await _dbContext.AgentTasks.FindAsync([command.Id], cancellationToken);
        if (agentTask == null)
            throw new InvalidOperationException($"AgentTask with ID {command.Id} not found.");

        agentTask.TransitionStatus(command.Status);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteAgentTaskHandler : ICommandHandler<DeleteAgentTaskCommand>
{
    private readonly DevStackDbContext _dbContext;

    public DeleteAgentTaskHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteAgentTaskCommand command, CancellationToken cancellationToken)
    {
        var agentTask = await _dbContext.AgentTasks.FindAsync([command.Id], cancellationToken);
        if (agentTask == null)
            throw new InvalidOperationException($"AgentTask with ID {command.Id} not found.");

        _dbContext.AgentTasks.Remove(agentTask);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class GetAgentTaskByIdHandler : ICommandHandler<AgentTask, GetAgentTaskByIdQuery>
{
    private readonly DevStackDbContext _dbContext;

    public GetAgentTaskByIdHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AgentTask> Handle(GetAgentTaskByIdQuery query, CancellationToken cancellationToken = default)
    {
        var agentTask = await _dbContext.AgentTasks.FindAsync([query.Id], cancellationToken);
        if (agentTask == null)
        {
            throw new InvalidOperationException($"AgentTask with ID {query.Id} not found.");
        }
        return agentTask;
    }
}
