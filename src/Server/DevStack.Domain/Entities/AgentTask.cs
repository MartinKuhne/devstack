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

    // DRIFT-01 — the status machine is not applied in the write path: this assigns unconditionally.
    // Category:        invariant-violation (documented rules are not enforced)
    // Trigger path:    updateAgentTaskStatus / update_task_status
    //                  -> UpdateAgentTaskStatusHandler calls agentTask.TransitionStatus(command.Status)
    //                  -> this method assigns Status with no guard, and SaveChangesAsync commits it
    // Why this is a real bug: the same pattern as Deliverable.TransitionStatus - StatusTransitionService holds
    //                  the transition rules and is never consulted by the write path, so any status can be
    //                  assigned to any task (e.g. Done -> Ready).
    // Why this is NOT a false positive: the integration suite performs an illegal task transition against a
    //                  running server and observes it succeed, and the tool description advertises the guard
    //                  ("The state machine enforces valid transitions", Descriptions.TaskTools.UpdateTaskStatus).
    // Concrete consequence: task history is not trustworthy - completed tasks can be silently reopened and
    //                  deliverables whose auto-completion depends on task status can flip back.
    // Confidence:      Confirmed.
    // Fix direction:   validate before assigning (or drop the unused service and the advertised guarantee).
    //                  Product-owner decision recorded as DRIFT-01.
    // Pinned by:       DevStack.Tests.Integration.GraphQL (task status-transition scenarios assert the
    //                  permissive behaviour); they turn red when a guard is added.
    public void TransitionStatus(AgentTaskStatus newStatus)
    {
        Status = newStatus;
    }
}
