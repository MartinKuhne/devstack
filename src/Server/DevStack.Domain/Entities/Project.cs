using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevStack.Domain.Entities;

public class Project : Entity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Architecture { get; set; }

    [Required]
    public string Memory { get; set; } = string.Empty;

    public Uri? GithubUrl { get; set; }

    public string? GithubToken_Encrypted { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    [Obsolete("Use Items instead")]
    [NotMapped]
    public virtual ICollection<Item> Features => Items;

    public virtual ICollection<Epic> Epics { get; set; } = new List<Epic>();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Project()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
