using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.Persistence.Configurations;

public class AgentTaskConfiguration : IEntityTypeConfiguration<DevStack.Domain.Entities.AgentTask>
{
    public void Configure(EntityTypeBuilder<DevStack.Domain.Entities.AgentTask> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(300);
            
        builder.Property(t => t.Status)
            .IsRequired();
            
        builder.Property(t => t.Deliverable)
            .IsRequired(false);
            
        builder.Property(t => t.AcceptanceCriteria)
            .IsRequired(false);
            
        builder.Property(t => t.Risks)
            .IsRequired(false);
            
        builder.Property(t => t.Result)
            .IsRequired(false);
            
        builder.Property(t => t.RequiredFollowUps)
            .IsRequired(false);
            
        builder.Property(t => t.ComplexityRating)
            .IsRequired()
            .HasMaxLength(2); // For values 1-10
            
        builder.HasOne(t => t.Feature)
            .WithMany(f => f.Tasks)
            .HasForeignKey(t => t.FeatureId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(t => new { t.FeatureId, t.Status });
    }
}