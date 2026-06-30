using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilotAI.Domain.Entities;

namespace TubePilotAI.Infrastructure.Persistence.Configurations;

public sealed class ContentGenerationJobConfiguration : IEntityTypeConfiguration<ContentGenerationJob>
{
    public void Configure(EntityTypeBuilder<ContentGenerationJob> builder)
    {
        builder.ToTable("ContentGenerationJobs");

        builder.HasKey(job => job.Id);

        builder.Property(job => job.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(job => job.PromptText)
            .HasMaxLength(8000)
            .IsRequired();

        builder.Property(job => job.SelectedProvidersJson)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(job => job.Status)
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(job => job.ExportFolderPath)
            .HasMaxLength(500);

        builder.Property(job => job.ResultJson)
            .IsRequired();

        builder.Property(job => job.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(job => job.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(job => job.CompletedAtUtc)
            .HasColumnType("datetime2");

        builder.HasOne(job => job.PromptTemplate)
            .WithMany()
            .HasForeignKey(job => job.PromptTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(job => job.Title);
        builder.HasIndex(job => job.Status);
        builder.HasIndex(job => job.CreatedAtUtc);
    }
}
