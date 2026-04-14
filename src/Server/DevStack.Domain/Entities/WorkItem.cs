using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Entities;

public abstract class WorkItem : Entity
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public FeatureStatus Status { get; set; }

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

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    protected WorkItem()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
