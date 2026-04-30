namespace DevStack.Domain.Entities;

public class Deliverable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    [Required]
    public DeliverableType Type { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DeliverableStatus Status { get; set; }

    public string? Description { get; set; }

    public string? Design { get; set; }

    public string? AcceptanceCriteria { get; set; }

    public string? ExecutionPlan { get; set; }

    public string? AgentFeedback { get; set; }

    public string? SecurityImpact { get; set; }

    public string? PerformanceImpact { get; set; }

    public string? TestPlan { get; set; }

    public string? DeploymentPlan { get; set; }

    public string? Blocking { get; set; }

    public virtual ICollection<AgentTask> AgentTasks { get; set; } = new List<AgentTask>();

    public Deliverable()
    {
    }
}
