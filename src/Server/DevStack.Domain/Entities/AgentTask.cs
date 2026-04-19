using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Entities;

public class AgentTask
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid DeliverableId { get; set; }

    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public AgentTaskStatus Status { get; set; }

    public string? Result { get; set; }

    public string? Errors { get; set; }

    public string? CommitHash { get; set; }

    public int ComplexityRating { get; set; } = 1;

    public string? DependsOnDevTask { get; set; }

    public int? PromptTokens { get; set; }

    public int? CompletionTokens { get; set; }

    public double? ExecutionDurationInSeconds { get; set; }

    public string? Model { get; set; }

    [ForeignKey(nameof(DeliverableId))]
    public virtual Deliverable? Deliverable { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    public AgentTask()
    {
    }

    public void SetComplexityRating(int rating)
    {
        if (rating < 1 || rating > 10)
            throw new ArgumentException("Complexity rating must be between 1 and 10.", nameof(rating));
        ComplexityRating = rating;
    }
}
