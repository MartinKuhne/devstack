using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevStack.Domain.Entities;

public class Feature : WorkItem
{
    public virtual ICollection<AgentTask> Tasks { get; set; } = new List<AgentTask>();
}
