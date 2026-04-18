using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Persistence.Configurations;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(a => a.EntityId)
            .IsRequired();
            
        builder.Property(a => a.EventType)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(a => a.OldValue)
            .IsRequired(false);
            
        builder.Property(a => a.NewValue)
            .IsRequired(false);
            
        builder.Property(a => a.Actor)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(a => a.OccurredAt)
            .IsRequired();
    }
}