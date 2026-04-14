using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.Persistence.Configurations;

public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.Title)
            .IsRequired()
            .HasMaxLength(300);
            
        builder.Property(f => f.Status)
            .IsRequired();
            
        builder.Property(f => f.Description)
            .IsRequired(false);
            
        builder.Property(f => f.AcceptanceCriteria)
            .IsRequired(false);
            
        builder.Property(f => f.Plan)
            .IsRequired(false);
            
        builder.Property(f => f.SecurityImpact)
            .IsRequired(false);
            
        builder.Property(f => f.PerformanceImpact)
            .IsRequired(false);
            
        builder.Property(f => f.TestPlan)
            .IsRequired(false);
            
        builder.Property(f => f.DeploymentPlan)
            .IsRequired(false);
            
        builder.Property(f => f.OpenQuestions)
            .IsRequired(false);
            
        builder.Property(f => f.Result)
            .IsRequired(false);
            
        builder.Property(f => f.Errors)
            .IsRequired(false);
            
        builder.HasOne<Project>()
            .WithMany(p => p.Features)
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(f => f.Tasks)
            .WithOne()
            .HasForeignKey(t => t.FeatureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}