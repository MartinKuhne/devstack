using DevStack.Domain.Services;

namespace DevStack.Domain.Entities;

public class Deliverable
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; private set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    [Required]
    public DeliverableType Type { get; private set; }

    [Required]
    [StringLength(200)]
    public string Title { get; private set; } = string.Empty;

    [Required]
    public DeliverableStatus Status { get; private set; }

    public string? Description { get; private set; }

    public string? Design { get; private set; }

    public string? AcceptanceCriteria { get; private set; }

    public string? ExecutionPlan { get; private set; }

    public string? AgentFeedback { get; private set; }

    public string? SecurityImpact { get; private set; }

    public string? PerformanceImpact { get; private set; }

    public string? TestPlan { get; private set; }

    public string? DeploymentPlan { get; private set; }

    public string? Blocking { get; private set; }

    public virtual ICollection<AgentTask> AgentTasks { get; set; } = new List<AgentTask>();

    public Deliverable()
    {
    }

    public Deliverable(
        Guid projectId,
        DeliverableType type,
        string title,
        DeliverableStatus status = DeliverableStatus.Draft,
        string? description = null,
        string? design = null,
        string? acceptanceCriteria = null,
        string? executionPlan = null,
        string? securityImpact = null,
        string? performanceImpact = null,
        string? testPlan = null,
        string? deploymentPlan = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (title.Length > 200)
            throw new ArgumentException("Title must be 200 characters or less", nameof(title));

        Id = id.HasValue && id.Value != Guid.Empty ? id.Value : Guid.NewGuid();
        ProjectId = projectId;
        Type = type;
        Title = title;
        Status = status;
        Description = description;
        Design = design;
        AcceptanceCriteria = acceptanceCriteria;
        ExecutionPlan = executionPlan;
        SecurityImpact = securityImpact;
        PerformanceImpact = performanceImpact;
        TestPlan = testPlan;
        DeploymentPlan = deploymentPlan;
    }

    public void UpdateMetadata(
        string? title = null,
        string? description = null,
        string? acceptanceCriteria = null,
        string? executionPlan = null,
        string? agentFeedback = null,
        string? securityImpact = null,
        string? performanceImpact = null,
        string? testPlan = null,
        string? deploymentPlan = null,
        string? blocking = null,
        string? design = null)
    {
        if (title is not null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty", nameof(title));
            if (title.Length > 200)
                throw new ArgumentException("Title must be 200 characters or less", nameof(title));
            Title = title;
        }

        if (description is not null) Description = description;
        if (acceptanceCriteria is not null) AcceptanceCriteria = acceptanceCriteria;
        if (executionPlan is not null) ExecutionPlan = executionPlan;
        if (agentFeedback is not null) AgentFeedback = agentFeedback;
        if (securityImpact is not null) SecurityImpact = securityImpact;
        if (performanceImpact is not null) PerformanceImpact = performanceImpact;
        if (testPlan is not null) TestPlan = testPlan;
        if (deploymentPlan is not null) DeploymentPlan = deploymentPlan;
        if (blocking is not null) Blocking = blocking;
        if (design is not null) Design = design;
    }

    public void TransitionStatus(DeliverableStatus targetStatus)
    {
        if (!StatusTransitionService.CanTransition(Status, targetStatus))
        {
            throw new InvalidOperationException($"Invalid status transition from {Status} to {targetStatus}.");
        }
        Status = targetStatus;
    }
}
