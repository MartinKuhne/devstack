using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.Entities;

public class Epic : Entity
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public virtual ICollection<Feature> Features { get; set; } = new List<Feature>();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Epic()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
