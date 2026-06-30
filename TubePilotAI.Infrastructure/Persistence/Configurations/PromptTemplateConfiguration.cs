using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilotAI.Domain.Entities;

namespace TubePilotAI.Infrastructure.Persistence.Configurations;

public sealed class PromptTemplateConfiguration : IEntityTypeConfiguration<PromptTemplate>
{
    public void Configure(EntityTypeBuilder<PromptTemplate> builder)
    {
        builder.ToTable("PromptTemplates");

        builder.HasKey(promptTemplate => promptTemplate.Id);

        builder.Property(promptTemplate => promptTemplate.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(promptTemplate => promptTemplate.Category)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(promptTemplate => promptTemplate.Description)
            .HasMaxLength(500);

        builder.Property(promptTemplate => promptTemplate.TemplateText)
            .HasMaxLength(8000)
            .IsRequired();

        builder.Property(promptTemplate => promptTemplate.SystemMessage)
            .HasMaxLength(2000);

        builder.Property(promptTemplate => promptTemplate.VariablesJson)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(promptTemplate => promptTemplate.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(promptTemplate => promptTemplate.IsDefault)
            .IsRequired();

        builder.Property(promptTemplate => promptTemplate.CreatedBy)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(promptTemplate => promptTemplate.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(promptTemplate => promptTemplate.UpdatedAtUtc)
            .HasColumnType("datetime2");

        builder.HasIndex(promptTemplate => promptTemplate.Name)
            .IsUnique();

        builder.HasIndex(promptTemplate => promptTemplate.Category);
        builder.HasIndex(promptTemplate => promptTemplate.Status);
        builder.HasIndex(promptTemplate => promptTemplate.CreatedAtUtc);
        builder.HasIndex(promptTemplate => promptTemplate.IsDefault);
    }
}
