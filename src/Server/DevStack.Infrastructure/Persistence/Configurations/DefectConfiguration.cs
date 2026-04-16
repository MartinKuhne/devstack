using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.Persistence.Configurations;

public class DefectConfiguration : IEntityTypeConfiguration<Defect>
{
    public void Configure(EntityTypeBuilder<Defect> builder)
    {
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(300);
            
        builder.Property(d => d.Status)
            .IsRequired();
            
        builder.Property(d => d.Description)
            .IsRequired(false);
            
        builder.Property(d => d.AcceptanceCriteria)
            .IsRequired(false);
            
        builder.Property(d => d.Plan)
            .IsRequired(false);
            
        builder.Property(d => d.SecurityImpact)
            .IsRequired(false);
            
        builder.Property(d => d.PerformanceImpact)
            .IsRequired(false);
            
        builder.Property(d => d.TestPlan)
            .IsRequired(false);
            
        builder.Property(d => d.DeploymentPlan)
            .IsRequired(false);
            
        builder.Property(d => d.OpenQuestions)
            .IsRequired(false);
            
        builder.Property(d => d.Result)
            .IsRequired(false);
            
        builder.Property(d => d.Errors)
            .IsRequired(false);

        builder.Property(d => d.RootCause)
            .IsRequired(false);
            
        builder.Property(d => d.Severity)
            .IsRequired();
            
        builder.HasOne<Project>()
            .WithMany(p => p.Defects)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(d => d.ParentFeature)
            .WithMany()
            .HasForeignKey(d => d.ParentFeatureId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasIndex(d => new { d.ProjectId, d.Status });
    }
}