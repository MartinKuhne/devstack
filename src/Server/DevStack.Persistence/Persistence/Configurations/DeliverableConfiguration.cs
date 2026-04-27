using DevStack.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Persistence.Configurations;

public class DeliverableConfiguration : IEntityTypeConfiguration<Deliverable>
{
    public void Configure(EntityTypeBuilder<Deliverable> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Status)
            .IsRequired();

        builder.Property(d => d.Type)
            .IsRequired();

        builder.Property(d => d.Description)
            .IsRequired(false);

        builder.Property(d => d.Design)
            .IsRequired(false);

        builder.Property(d => d.AcceptanceCriteria)
            .IsRequired(false);

        builder.Property(d => d.ExecutionPlan)
            .IsRequired(false);

        builder.Property(d => d.AgentFeedback)
            .IsRequired(false);

        builder.Property(d => d.SecurityImpact)
            .IsRequired(false);

        builder.Property(d => d.PerformanceImpact)
            .IsRequired(false);

        builder.Property(d => d.TestPlan)
            .IsRequired(false);

        builder.Property(d => d.DeploymentPlan)
            .IsRequired(false);

        builder.Property(d => d.Blocking)
            .IsRequired(false);

        builder.HasOne(d => d.Project)
            .WithMany(p => p.Deliverables)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.AgentTasks)
            .WithOne(t => t.Deliverable)
            .HasForeignKey(t => t.DeliverableId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
