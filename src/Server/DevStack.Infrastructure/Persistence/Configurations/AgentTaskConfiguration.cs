#nullable disable warnings
#pragma warning disable CS0618
using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.Persistence.Configurations;

public class AgentTaskConfiguration : IEntityTypeConfiguration<AgentTask>
{
    public void Configure(EntityTypeBuilder<AgentTask> builder)
    {
        builder.HasKey(at => at.Id);

        builder.Property(at => at.ProjectId)
            .IsRequired();

        builder.Property(at => at.ItemId)
            .IsRequired();

        builder.Property(at => at.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(at => at.Status)
            .IsRequired();

        builder.Property(at => at.Deliverable)
            .IsRequired(false);

        builder.Property(at => at.AcceptanceCriteria)
            .IsRequired(false);

        builder.Property(at => at.Risks)
            .IsRequired(false);

        builder.Property(at => at.Result)
            .IsRequired(false);

        builder.Property(at => at.RequiredFollowUps)
            .IsRequired(false);

        builder.Property(at => at.ComplexityRating)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(at => at.Errors)
            .IsRequired(false);

        builder.Property(at => at.CommitHash)
            .IsRequired(false);

        builder.Property(at => at.DependsOnAgentTask)
            .IsRequired(false);

        builder.Property(at => at.PromptTokens)
            .IsRequired(false);

        builder.Property(at => at.CompletionTokens)
            .IsRequired(false);

        builder.Property(at => at.ExecutionDurationInSeconds)
            .IsRequired(false);

        builder.Property(at => at.Model)
            .IsRequired(false);

        builder.HasOne(at => at.Item)
            .WithMany()
            .HasForeignKey(at => at.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
#pragma warning restore CS0618
