using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.Entities;

public class AuditEvent : Entity
{
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    [Required]
    [StringLength(200)]
    public string Actor { get; set; } = string.Empty;

    [Required]
    public DateTime OccurredAt { get; set; }

    public AuditEvent()
    {
        Id = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }
}
