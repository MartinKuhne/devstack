using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Entities;

public class Defect : WorkItem
{
    public Guid? ParentFeatureId { get; set; }

    public Severity? Severity { get; set; }

    public string? RootCause { get; set; }

    public virtual Feature? ParentFeature { get; set; }
}
