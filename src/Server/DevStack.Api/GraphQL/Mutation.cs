using DevStack.Application.AgentTasks.Commands;
using DevStack.Application.Deliverables.Commands;
using DevStack.Application.LargeLanguageModels.Commands;
using DevStack.Application.Projects.Commands;
using DevStack.Domain.Exceptions;
using DevStack.Domain.Services;
using DevStack.Infrastructure.AgentTasks;

namespace DevStack.Api.GraphQL.Types;

public record CreateProjectInput(
    string Name,
    string Repository,
    string? Description
);

public record UpdateProjectInput(
    Guid Id,
    string? Name,
    string? Description,
    string? Repository);

public record CreateDeliverableInput(
    Guid ProjectId,
    string Title,
    string Type,
    string Description,
    DeliverableStatus InitialStatus,
    string? AcceptanceCriteria,
    string? ExecutionPlan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? Design
    );

public record UpdateDeliverableInput(
    Guid Id,
    string? Title,
    string? Description,
    string? AcceptanceCriteria,
    string? AgentFeedback,
    string? ExecutionPlan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? Blocking,
    string? Design);

public record UpdateDeliverableStatusInput(
    Guid Id,
    DeliverableStatus TargetStatus,
    string? Actor);

public record CreateAgentTaskInput(
    Guid DeliverableId,
    Guid ProjectId,
    string Title,
    string Description,
    Guid? DependsOnAgentTaskId,
        int ComplexityRating = 5
);

public record UpdateAgentTaskInput(
    Guid Id,
    string? Title,
    string? Description,
    string? Result,
    string? Errors,
    string? CommitHash,
    Guid? DependsOnAgentTaskId,
    int? ComplexityRating,
    int? PromptTokens,
    int? CompletionTokens,
    int? ExecutionDurationInSeconds,
    string? Agent);

public record UpdateAgentTaskStatusInput(
    Guid Id,
    AgentTaskStatus TargetStatus,
    string Actor);

public record CreateLargeLanguageModelInput(
    string Url,
    string Model,
    string? ModelAlias,
    string? ApiKey,
    int Cost = 0,
    int MaxComplexity = 10,
    int MaxConcurrency = 1);

public record UpdateLargeLanguageModelInput(
    Guid Id,
    string? Url,
    string? Model,
    string? ModelAlias,
    string? ApiKey,
    int? Cost,
    int? MaxComplexity,
    int? MaxConcurrency);

public record DeleteTestDataPayload(bool Success, string? Message);

public class Mutation
{
    public async Task<Project?> CreateProjectAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<Guid, CreateProjectCommand> handler,
        CreateProjectInput input,
        CancellationToken cancellationToken)
    {
        var id = await handler.Handle(new CreateProjectCommand(
            input.Name,
            input.Description,
            input.Repository), cancellationToken);

        var project = await dbContext.Projects.FindAsync(id, cancellationToken);
        return project;
    }

    public async Task<Project?> UpdateProjectAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<UpdateProjectCommand> handler,
        UpdateProjectInput input,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UpdateProjectCommand(
            input.Id,
            input.Name,
            input.Description,
            input.Repository), cancellationToken);

        var project = await dbContext.Projects.FindAsync(input.Id, cancellationToken);
        return project;
    }

    public async Task<bool> DeleteProjectAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<DeleteProjectCommand> handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new DeleteProjectCommand(id), cancellationToken);
        return true;
    }

    public async Task<Deliverable?> CreateDeliverableAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<Guid, CreateDeliverableCommand> handler,
        CreateDeliverableInput input,
        CancellationToken cancellationToken)
    {
        // DRIFT-10 — Enum.Parse accepts numeric strings, so an invalid type is not rejected here.
        // Category:        validation-bypass (late failure instead of input validation)
        // Trigger path:    createDeliverable(input: { type: "999", ... })
        //                  -> this line calls Enum.Parse(typeof(DeliverableType), "999", ignoreCase: true)
        //                  -> Enum.Parse parses any numeric string into the underlying int and does NOT check
        //                     that the value is a declared member, so the cast succeeds with (DeliverableType)999
        //                  -> the handler persists the row and SaveChangesAsync commits it
        //                  -> the failure only appears later, when HotChocolate serialises the enum member and
        //                     cannot map it: the response is an EXEC_INVALID_LEAF_VALUE execution error
        // Why this is a real bug: the mutation reports a serialization fault rather than an input-validation
        //                  fault for a bad argument, and the deliverable row has already been written with an
        //                  undefined type value that no client can render.
        // Why this is NOT a false positive: the behaviour is observed end to end - the integration scenario
        //                  sends a numeric type string and receives the serialization error rather than a
        //                  validation error naming the field. Declaring the GraphQL argument as the enum type
        //                  (DeliverableType instead of String) would make HotChocolate reject it up front, which
        //                  is the fix below.
        // Concrete consequence: an orphan deliverable with an unrenderable type in the database plus an error
        //                  message that points the caller at the wrong layer.
        // Confidence:      Confirmed.
        // Fix direction:   declare CreateDeliverableInput.Type as DeliverableType and let HotChocolate bind it,
        //                  or use Enum.TryParse<T>(value, ignoreCase: true, out var t) && Enum.IsDefined(t).
        // Pinned by:       DevStack.Tests.Integration.GraphQL Features/GraphQLErrorPaths.feature
        //                  "A numeric deliverable type string slips past Enum.Parse and fails serialization".
        //                  That scenario expects the serialization error today and must be rewritten when the
        //                  input is validated properly.
        var deliverableType = (DeliverableType)Enum.Parse(typeof(DeliverableType), input.Type, ignoreCase: true);

        var id = await handler.Handle(new CreateDeliverableCommand(
            input.ProjectId,
            deliverableType,
            input.Title,
            input.Description,
            input.AcceptanceCriteria,
            input.ExecutionPlan,
            input.SecurityImpact,
            input.PerformanceImpact,
            input.TestPlan,
            input.DeploymentPlan,
            input.InitialStatus,
            input.Design), cancellationToken);

        return await dbContext.Deliverables.FindAsync([id], cancellationToken);
    }

    public async Task<Deliverable> UpdateDeliverableAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<UpdateDeliverableCommand> handler,
        UpdateDeliverableInput input,
        CancellationToken cancellationToken)
    {
        // BUG-07 — updateDeliverable swaps agentFeedback and executionPlan on the way to the entity.
        // Category:        data-corruption
        // Trigger path:    client sends updateDeliverable(input:{agentFeedback:"A", executionPlan:"B"})
        //                  -> UpdateDeliverableInput declares AgentFeedback at position 5, ExecutionPlan at 6
        //                     (Mutation.cs:43-44)
        //                  -> this call passes the arguments POSITIONALLY, so "A" lands in slot 5 and "B" in 6
        //                  -> UpdateDeliverableCommand declares ExecutionPlan at position 5, AgentFeedback at 6
        //                     (UpdateDeliverableCommand.cs:8-9), so "A" becomes command.ExecutionPlan
        //                  -> UpdateDeliverableHandler writes command.ExecutionPlan into Deliverable.AgentFeedback
        //                     and vice versa; SaveChangesAsync commits the crossed values.
        // Why this is a real bug: both parameters are string?, so the compiler cannot detect the crossing, and
        //                  the mutation reports success. Every caller that sets both fields has its data swapped
        //                  in the database, and re-querying returns the crossed values consistently, so the write
        //                  looks correct to the client.
        // Why this is NOT a false positive: the two records were read side by side (Mutation.cs:38-50 vs
        //                  UpdateDeliverableCommand.cs:3-15) and the call is positional, not named. The sibling
        //                  command constructions in this file (CreateDeliverableCommand, UpdateAgentTaskCommand,
        //                  both LargeLanguageModel commands, both Project commands) were audited and DO match
        //                  their record order, so this is a single crossed pair rather than a convention.
        // Concrete consequence: agent feedback and execution plans are persisted in each other's columns and
        //                  are unrecoverable without direct SQL; the AdminUi renders each under the wrong label.
        // Confidence:      Confirmed (traced end-to-end; pinned by an integration scenario that observes the
        //                  crossed columns in the persisted row).
        // Fix direction:   pass the arguments by name:
        //                  new UpdateDeliverableCommand(input.Id, Title: input.Title, ...,
        //                      AgentFeedback: input.AgentFeedback, ExecutionPlan: input.ExecutionPlan, ...).
        //                  Prefer named arguments for every command construction in this file as defence.
        // Pinned by:       DevStack.Tests.Integration.GraphQL Features/GraphQLFieldCoverage.feature
        //                  scenario "GQL-13 DEFECT updateDeliverable crosses agentFeedback and executionPlan".
        //                  That scenario turns red as soon as this is fixed - update or delete it then.
        await handler.Handle(new UpdateDeliverableCommand(
            input.Id,
            input.Title,
            input.Description,
            input.AcceptanceCriteria,
            input.AgentFeedback,
            input.ExecutionPlan,
            input.SecurityImpact,
            input.PerformanceImpact,
            input.TestPlan,
            input.DeploymentPlan,
            input.Blocking,
            input.Design), cancellationToken);

        var deliverable = await dbContext.Deliverables.FindAsync([input.Id], cancellationToken);
        if (deliverable == null)
        {
            throw new InvalidOperationException($"Deliverable with ID {input.Id} not found.");
        }
        return deliverable;
    }

    public async Task<DeliverableStatus> UpdateDeliverableStatusAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<UpdateDeliverableStatusCommand> handler,
        Guid id,
        DeliverableStatus targetStatus,
        string? actor,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UpdateDeliverableStatusCommand(id, targetStatus, actor ?? string.Empty), cancellationToken);
        var deliverable = await dbContext.Deliverables.FindAsync([id], cancellationToken);
        return deliverable?.Status ?? targetStatus;
    }

    public async Task<bool> DeleteDeliverableAsync(
        [Service] ICommandHandler<DeleteDeliverableCommand> handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new DeleteDeliverableCommand(id), cancellationToken);
        return true;
    }

    public async Task<AgentTask> CreateAgentTaskAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<Guid, CreateAgentTaskCommand> handler,
        CreateAgentTaskInput input,
        CancellationToken cancellationToken)
    {
        var id = await handler.Handle(new CreateAgentTaskCommand(
            input.ProjectId,
            input.DeliverableId,
            input.Title,
            input.Description,
            input.ComplexityRating,
            input.DependsOnAgentTaskId), cancellationToken);

        var agentTask = await dbContext.AgentTasks.FindAsync([id], cancellationToken);
        if (agentTask == null)
        {
            throw new InvalidOperationException("Failed to create agent task");
        }

        return agentTask;
    }

    public async Task<AgentTask> UpdateAgentTaskAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<UpdateAgentTaskCommand> handler,
        UpdateAgentTaskInput input,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UpdateAgentTaskCommand(
            input.Id,
            input.Title,
            input.Description,
            input.Result,
            input.Errors,
            input.CommitHash,
            input.ComplexityRating,
            input.DependsOnAgentTaskId,
            input.PromptTokens,
            input.CompletionTokens,
            input.ExecutionDurationInSeconds,
            input.Agent), cancellationToken);

        var agentTask = await dbContext.AgentTasks.FindAsync([input.Id], cancellationToken);
        if (agentTask == null)
        {
            throw new InvalidOperationException("AgentTask does not exist");
        }

        return agentTask;
    }

    public async Task<AgentTaskStatus> UpdateAgentTaskStatusAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<UpdateAgentTaskStatusCommand> taskHandler,
        [Service] ICommandHandler<UpdateDeliverableStatusCommand> statusHandler,
        Guid id,
        AgentTaskStatus targetStatus,
        string? actor,
        CancellationToken cancellationToken)
    {
        await taskHandler.Handle(new UpdateAgentTaskStatusCommand(id, targetStatus, actor ?? string.Empty), cancellationToken);

        var agentTask = await dbContext.AgentTasks.FindAsync(id, cancellationToken);
        if (agentTask == null)
        {
            throw new InvalidOperationException("AgentTask does not exist");
        }

        if (targetStatus == AgentTaskStatus.Done)
        {
            await CheckAndMarkDeliverableDoneAsync(dbContext, statusHandler, agentTask.DeliverableId, cancellationToken);
        }

        return targetStatus;
    }

    public async Task<bool> DeleteAgentTaskAsync(
        [Service] ICommandHandler<DeleteAgentTaskCommand> handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new DeleteAgentTaskCommand(id), cancellationToken);
        return true;
    }

    public async Task<LargeLanguageModel?> CreateLargeLanguageModelAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<Guid, CreateLargeLanguageModelCommand> handler,
        CreateLargeLanguageModelInput input,
        CancellationToken cancellationToken)
    {
        var id = await handler.Handle(new CreateLargeLanguageModelCommand(
            input.Url,
            input.Model,
            input.ModelAlias,
            input.ApiKey ?? String.Empty,
            input.Cost,
            input.MaxComplexity,
            input.MaxConcurrency), cancellationToken);

        var model = await dbContext.LargeLanguageModels.FindAsync(id, cancellationToken);
        return model;
    }

    public async Task<LargeLanguageModel?> UpdateLargeLanguageModelAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<UpdateLargeLanguageModelCommand> handler,
        UpdateLargeLanguageModelInput input,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UpdateLargeLanguageModelCommand(
            input.Id,
            input.Url,
            input.Model,
            input.ModelAlias,
            input.ApiKey,
            input.Cost,
            input.MaxComplexity,
            input.MaxConcurrency), cancellationToken);

        var model = await dbContext.LargeLanguageModels.FindAsync(input.Id, cancellationToken);
        return model;
    }

    public async Task<bool> DeleteLargeLanguageModelAsync(
        [Service] ICommandHandler<DeleteLargeLanguageModelCommand> handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new DeleteLargeLanguageModelCommand(id), cancellationToken);
        return true;
    }

    public async Task<DeleteTestDataPayload> DeleteTestDataAsync(
        [Service] DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            await context.CleanupTestDataAsync(cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new DeleteTestDataPayload(true, "Test data cleaned up successfully");
        }
        catch (Exception ex)
        {
            return new DeleteTestDataPayload(false, ex.Message);
        }
    }

    public async Task<bool> CheckAndMarkDeliverableDoneAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<UpdateDeliverableStatusCommand> statusHandler,
        Guid deliverableId,
        CancellationToken cancellationToken)
    {
        var allTasks = await dbContext.AgentTasks
            .Where(t => t.DeliverableId == deliverableId)
            .ToListAsync(cancellationToken);

        if (DeliverableCompletionService.CheckAllTasksDone(allTasks))
        {
            await statusHandler.Handle(new UpdateDeliverableStatusCommand(deliverableId, DeliverableStatus.Done, "System"), cancellationToken);
            return true;
        }

        return false;
    }
}
