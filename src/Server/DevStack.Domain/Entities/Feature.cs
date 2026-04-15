using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevStack.Domain.Entities;

public class Feature : WorkItem
{
    public Guid? EpicId { get; set; }

    [ForeignKey(nameof(EpicId))]
    public virtual Epic? Epic { get; set; }

    public virtual ICollection<AgentTask> Tasks { get; set; } = new List<AgentTask>();
}
