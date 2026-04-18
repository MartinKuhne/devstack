using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevStack.Domain.Entities;

public class LargeLanguageModel : Entity
{
    [Required]
    [StringLength(500)]
    public string Url { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Model { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ModelAlias { get; set; }

    [Required]
    [StringLength(1000)]
    public string ApiKey_Encrypted { get; set; } = string.Empty;

    [Required]
    public int MaxComplexity { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    public LargeLanguageModel()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
