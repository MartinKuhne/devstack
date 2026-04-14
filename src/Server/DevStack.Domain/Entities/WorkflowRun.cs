using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Entities;

public class WorkflowRun : Entity
{
    [Required]
    public Guid ProjectId { get; set; }

    public Guid? FeatureId { get; set; }

    public Guid? TaskId { get; set; }

    [Required]
    public WorkflowType WorkflowType { get; set; }

    [Required]
    public WorkflowRunStatus Status { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public string? InputPayload { get; set; }

    public string? OutputPayload { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Project? Project { get; set; }
    public virtual Feature? Feature { get; set; }
    public virtual AgentTask? Task { get; set; }

    public WorkflowRun()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        StartedAt = DateTime.UtcNow;
    }
}
