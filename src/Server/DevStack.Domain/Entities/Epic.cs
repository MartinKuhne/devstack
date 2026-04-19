using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.Entities;

public class Epic
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    public Project? Project { get; set; }

    public Epic()
    {
    }
}
