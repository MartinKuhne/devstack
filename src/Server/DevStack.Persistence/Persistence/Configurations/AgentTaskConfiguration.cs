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

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.DependsOnAgentTaskId)
            .IsRequired(false);

        builder.Property(t => t.PromptTokens)
            .IsRequired(false);

        builder.Property(t => t.CompletionTokens)
            .IsRequired(false);

        builder.Property(t => t.ExecutionDurationInSeconds)
            .IsRequired(false);

        builder.Property(t => t.Agent)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.HasOne(t => t.DependsOnAgentTask)
            .WithMany()
            .HasForeignKey(t => t.DependsOnAgentTaskId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
