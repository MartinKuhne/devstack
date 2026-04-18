using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.Persistence.Configurations;

public class WorkflowRunConfiguration : IEntityTypeConfiguration<WorkflowRun>
{
    public void Configure(EntityTypeBuilder<WorkflowRun> builder)
    {
        builder.HasKey(w => w.Id);
        
        builder.Property(w => w.WorkflowType)
            .IsRequired();
            
        builder.Property(w => w.Status)
            .IsRequired();
            
        builder.Property(w => w.StartedAt)
            .IsRequired();
            
        builder.Property(w => w.CompletedAt)
            .IsRequired(false);
            
        builder.Property(w => w.ErrorMessage)
            .IsRequired(false);
            
        builder.Property(w => w.InputPayload)
            .IsRequired(false);
            
        builder.Property(w => w.OutputPayload)
            .IsRequired(false);
            
        builder.Property(w => w.CreatedAt)
            .IsRequired();
            
        builder.HasOne(w => w.Project)
            .WithMany()
            .HasForeignKey(w => w.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(w => w.Item)
            .WithMany()
            .HasForeignKey(w => w.ItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
