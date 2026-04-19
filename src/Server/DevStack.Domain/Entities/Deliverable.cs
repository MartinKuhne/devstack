using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Entities;

public class Deliverable : Entity
{
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

    public string? AcceptanceCriteria { get; set; }

    public string? Plan { get; set; }

    public string? SecurityImpact { get; set; }

    public string? PerformanceImpact { get; set; }

    public string? TestPlan { get; set; }

    public string? DeploymentPlan { get; set; }

    public string? OpenQuestions { get; set; }

    public string? Result { get; set; }

    public string? Errors { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public Severity Severity { get; set; }

    public string? RootCause { get; set; }

    public Deliverable()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
