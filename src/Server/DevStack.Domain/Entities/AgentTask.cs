using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Entities;

public class AgentTask : Entity
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid ItemId { get; set; }

    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public global::DevStack.Domain.Enums.TaskStatus Status { get; set; }

    public string? Deliverable { get; set; }

    public string? AcceptanceCriteria { get; set; }

    public string? Risks { get; set; }

    public string? Result { get; set; }

    public string? RequiredFollowUps { get; set; }

    [Required]
    public int ComplexityRating { get; set; } = 1;

    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(ItemId))]
    public virtual Item? Item { get; set; }

    [Obsolete("Use ItemId and Item instead")]
    public Guid FeatureId { get => ItemId; set => ItemId = value; }

    [Obsolete("Use Item instead")]
    [NotMapped]
    public virtual Item? Feature { get => Item; set => Item = value; }

    public DateTime UpdatedAt { get; set; }

    public AgentTask()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetComplexityRating(int rating)
    {
        if (rating < 1 || rating > 10)
            throw new ArgumentException("Complexity rating must be between 1 and 10.", nameof(rating));
        ComplexityRating = rating;
    }
}
