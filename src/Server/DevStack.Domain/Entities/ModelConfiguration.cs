using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.Entities;

public class ModelConfiguration : Entity
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

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ModelConfiguration()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}