using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevStack.Domain.Entities;

public class Epic : Entity
{
    [Required]
    public Guid ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    [Obsolete("Use Items instead")]
    public virtual ICollection<Item> Features => Items;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Epic()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
