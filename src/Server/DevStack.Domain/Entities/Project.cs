using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [StringLength(500)]
    public string Repository { get; set; } = string.Empty;

    public virtual ICollection<Deliverable> Deliverables { get; set; } = new List<Deliverable>();

    public Project()
    {
    }
}
