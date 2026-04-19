using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevStack.Domain.Entities;

public class LargeLanguageModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

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
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public int MaxComplexity { get; set; }

    public int MaxConcurrency { get; set; }

    public LargeLanguageModel()
    {
    }
}
