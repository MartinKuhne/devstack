using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Persistence.Configurations;

public class AgentTaskConfiguration : IEntityTypeConfiguration<AgentTask>
{
    public void Configure(EntityTypeBuilder<AgentTask> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.Result)
            .IsRequired(false);

        builder.Property(t => t.Errors)
            .IsRequired(false);

        builder.Property(t => t.CommitHash)
            .IsRequired(false);

        builder.Property(t => t.ComplexityRating)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(t => t.DependsOnDevTask)
            .IsRequired(false);

        builder.Property(t => t.PromptTokens)
            .IsRequired(false);

        builder.Property(t => t.CompletionTokens)
            .IsRequired(false);

        builder.Property(t => t.ExecutionDurationInSeconds)
            .IsRequired(false);

        builder.Property(t => t.Model)
            .IsRequired(false);

        builder.HasOne(t => t.Deliverable)
            .WithMany()
            .HasForeignKey(t => t.DeliverableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
