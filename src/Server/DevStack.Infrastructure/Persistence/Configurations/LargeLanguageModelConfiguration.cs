using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.Persistence.Configurations;

public class LargeLanguageModelConfiguration : IEntityTypeConfiguration<LargeLanguageModel>
{
    public void Configure(EntityTypeBuilder<LargeLanguageModel> builder)
    {
        builder.HasKey(llm => llm.Id);

        builder.Property(llm => llm.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(llm => llm.Model)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(llm => llm.ModelAlias)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(llm => llm.ApiKey_Encrypted)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(llm => llm.MaxComplexity)
            .IsRequired();

        builder.Property(llm => llm.ProjectId)
            .IsRequired();

        builder.HasOne(llm => llm.Project)
            .WithMany()
            .HasForeignKey(llm => llm.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
