using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Entities;

public class Item : WorkItem
{
    [Required]
    public ItemSubtype ItemType { get; set; }

    [NotMapped]
    public ItemSubtype Subtype
    {
        get => ItemType;
        set => ItemType = value;
    }

    public virtual Project? Project { get; set; }

    [NotMapped]
    public virtual Item? ParentFeature
    {
        get => this;
        set { }
    }

 

    public Guid? ParentFeatureId { get; set; }

    public Severity? Severity { get; set; }

    public string? RootCause { get; set; }

    public Guid? DependsOnId { get; set; }

    public string? Deliverable { get; set; }

    public string? Risks { get; set; }

    public int ComplexityRating { get; set; } = 1;
}
