using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.Persistence.Configurations;

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

        builder.Property(i => i.Subtype)
            .IsRequired();
            
        builder.Property(i => i.ParentFeatureId)
            .IsRequired(false);
            
        builder.Property(i => i.Severity)
            .IsRequired(false);
            
        builder.Property(i => i.RootCause)
            .IsRequired(false);
            
        builder.HasOne<Project>()
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(i => i.Tasks)
            .WithOne()
            .HasForeignKey(t => t.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Epic)
            .WithMany(e => e.Items)
            .HasForeignKey(i => i.EpicId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.ParentFeature)
            .WithMany()
            .HasForeignKey(i => i.ParentFeatureId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasIndex(i => new { i.ProjectId, i.Status });
        builder.HasIndex(i => new { i.Subtype });
    }
}
