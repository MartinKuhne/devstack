using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Persistence.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(300);
            
        builder.Property(i => i.Status)
            .IsRequired();
            
        builder.Property(i => i.Description)
            .IsRequired(false);
            
        builder.Property(i => i.AcceptanceCriteria)
            .IsRequired(false);
            
        builder.Property(i => i.Plan)
            .IsRequired(false);
            
        builder.Property(i => i.SecurityImpact)
            .IsRequired(false);
            
        builder.Property(i => i.PerformanceImpact)
            .IsRequired(false);
            
        builder.Property(i => i.TestPlan)
            .IsRequired(false);
            
        builder.Property(i => i.DeploymentPlan)
            .IsRequired(false);
            
        builder.Property(i => i.OpenQuestions)
            .IsRequired(false);
            
        builder.Property(i => i.Result)
            .IsRequired(false);
            
        builder.Property(i => i.Errors)
            .IsRequired(false);

        builder.Property(i => i.ItemType)
            .IsRequired();
            
        builder.Property(i => i.ParentFeatureId)
            .IsRequired(false);
            
        builder.Property(i => i.Severity)
            .IsRequired(false);
            
        builder.Property(i => i.RootCause)
            .IsRequired(false);
            
        builder.Property(i => i.Deliverable)
            .IsRequired(false);
            
        builder.Property(i => i.Risks)
            .IsRequired(false);
            
        builder.Property(i => i.ComplexityRating)
            .IsRequired()
            .HasDefaultValue(1);
            
        builder.HasOne(i => i.Project)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}