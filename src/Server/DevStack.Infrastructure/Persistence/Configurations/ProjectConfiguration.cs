using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.Description)
            .IsRequired(false);
            
        builder.Property(p => p.Architecture)
            .IsRequired(false);
            
        builder.Property(p => p.Memory)
            .IsRequired(false);
            
        builder.Property(p => p.GithubUrl)
            .IsRequired(false);
            
        builder.Property(p => p.GithubToken_Encrypted)
            .IsRequired(false);
    }
}