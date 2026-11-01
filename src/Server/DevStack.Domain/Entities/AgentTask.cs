namespace DevStack.Domain.Entities;

public class AgentTask
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; private set; }

    [Required]
    public Guid DeliverableId { get; private set; }

    [Required]
    [StringLength(300)]
    public string Title { get; private set; } = string.Empty;

    [Required]
    public AgentTaskStatus Status { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public string? Result { get; private set; }

    public string? Errors { get; private set; }

    public string? CommitHash { get; private set; }

    public int ComplexityRating { get; private set; } = 1;

    public Guid? DependsOnAgentTaskId { get; private set; }

    public int? PromptTokens { get; private set; }

    public int? CompletionTokens { get; private set; }

    public int? ExecutionDurationInSeconds { get; private set; }

    public string? Agent { get; private set; }

    [ForeignKey(nameof(DeliverableId))]
    public virtual Deliverable? Deliverable { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    [ForeignKey(nameof(DependsOnAgentTaskId))]
    public virtual AgentTask? DependsOnAgentTask { get; set; }

    public AgentTask()
    {
    }

    public AgentTask(
        Guid projectId,
        Guid deliverableId,
        string title,
        string description = "",
        int complexityRating = 1,
        Guid? dependsOnAgentTaskId = null,
        AgentTaskStatus status = AgentTaskStatus.Ready,
        Guid? id = null,
        string? result = null,
        string? errors = null,
        string? commitHash = null,
        string? agent = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (title.Length > 300)
            throw new ArgumentException("Title must be 300 characters or less", nameof(title));

        Id = id.HasValue && id.Value != Guid.Empty ? id.Value : Guid.NewGuid();
        ProjectId = projectId;
        DeliverableId = deliverableId;
        Title = title;
        Description = description;
        DependsOnAgentTaskId = dependsOnAgentTaskId;
        Status = status;
        Result = result;
        Errors = errors;
        CommitHash = commitHash;
        Agent = agent;
        SetComplexityRating(complexityRating);
    }

    public void SetComplexityRating(int rating)
    {
        if (rating < 1 || rating > 10)
            throw new ArgumentException("Complexity rating must be between 1 and 10.", nameof(rating));
        ComplexityRating = rating;
    }

    public void UpdateMetadata(
        string? title = null,
        string? description = null,
        string? result = null,
        string? errors = null,
        string? commitHash = null,
        int? complexityRating = null,
        int? promptTokens = null,
        int? completionTokens = null,
        int? executionDurationInSeconds = null,
        string? agent = null)
    {
        if (title is not null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty", nameof(title));
            if (title.Length > 300)
                throw new ArgumentException("Title must be 300 characters or less", nameof(title));
            Title = title;
        }

        if (description is not null) Description = description;
        if (result is not null) Result = result;
        if (errors is not null) Errors = errors;
        if (commitHash is not null) CommitHash = commitHash;
        if (complexityRating.HasValue) SetComplexityRating(complexityRating.Value);
        if (promptTokens.HasValue) PromptTokens = promptTokens;
        if (completionTokens.HasValue) CompletionTokens = completionTokens;
        if (executionDurationInSeconds.HasValue) ExecutionDurationInSeconds = executionDurationInSeconds;
        if (agent is not null) Agent = agent;
    }

    // DRIFT-01 — this assigns unconditionally, and the tool description promises a guard that does not exist.
    // Category:        invariant-violation (advertised rule is not enforced)
    // Trigger path:    updateAgentTaskStatus / update_task_status
    //                  -> UpdateAgentTaskStatusHandler calls agentTask.TransitionStatus(command.Status)
    //                  -> this method assigns Status with no guard, and SaveChangesAsync commits it
    // Why this is a real bug: Descriptions.TaskTools.UpdateTaskStatus tells the caller "The state machine
    //                  enforces valid transitions", but nothing enforces anything on this path. Worse, there is
    //                  no task rule set to enforce: StatusTransitionService.CanTransition is overloaded only for
    //                  DeliverableStatus (StatusTransitionService.cs:9) and has no AgentTaskStatus equivalent, so
    //                  for tasks the advertised state machine does not exist in any form.
    // Why this is NOT a false positive: the guarantee is in the shipped tool description and the assignment is
    //                  unconditional, so the claim is false as written regardless of intent. The task
    //                  transitions pinned by TaskMutations.feature (Ready -> InProgress, InProgress ->
    //                  NeedsReview, NeedsReview -> Done, NeedsReview -> Rejected, ...) are therefore the only
    //                  de-facto contract, and they are pinned as free choices: the same mutations accept any
    //                  other target status too. Note this is drift rather than corruption for tasks precisely
    //                  because no rule set was ever written - the decision is whether to define one or to drop
    //                  the claim.
    // Concrete consequence: task history is not trustworthy - a completed task can be moved back to Ready or
    //                  forward to Done without any check, and deliverable auto-completion that keys off task
    //                  status reacts to a state the caller invented.
    // Confidence:      Confirmed.
    // Fix direction:   either define AgentTaskStatus transition rules and validate here, or remove the
    //                  "state machine enforces valid transitions" sentence from the tool description.
    //                  Product-owner decision recorded as DRIFT-01.
    // Pinned by:       DevStack.Tests.Integration.GraphQL Features/TaskMutations.feature (the transition
    //                  scenarios assert each requested status is accepted). If a guard is added, re-run them and
    //                  add the rejected-transition cases at the same time.
    public void TransitionStatus(AgentTaskStatus newStatus)
    {
        Status = newStatus;
    }
}
