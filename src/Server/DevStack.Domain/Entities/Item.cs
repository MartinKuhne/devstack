using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Entities;

public class Item : WorkItem
{
    [Required]
    public ItemSubtype Subtype { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    public Guid? EpicId { get; set; }

    [ForeignKey(nameof(EpicId))]
    public virtual Epic? Epic { get; set; }

    public virtual ICollection<AgentTask> Tasks { get; set; } = new List<AgentTask>();

    // Backward-compatible alias for Feature
    [NotMapped]
    public ItemSubtype FeatureSubtype => Subtype;

    // Properties migrated from Defect for backward compatibility
    public Guid? ParentFeatureId { get; set; }

    public Severity? Severity { get; set; }

    public string? RootCause { get; set; }

    [ForeignKey(nameof(ParentFeatureId))]
    public virtual Item? ParentFeature { get; set; }
}
