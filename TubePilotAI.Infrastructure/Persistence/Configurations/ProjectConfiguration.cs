using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilotAI.Domain.Entities;

namespace TubePilotAI.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(project => project.Description)
            .HasMaxLength(1200);

        builder.Property(project => project.OwnerName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(project => project.Status)
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(project => project.Priority)
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(project => project.Budget)
            .HasPrecision(18, 2);

        builder.Property(project => project.TagsJson)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(project => project.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(project => project.UpdatedAtUtc)
            .HasColumnType("datetime2");

        builder.HasIndex(project => project.Name).IsUnique();
        builder.HasIndex(project => project.OwnerName);
        builder.HasIndex(project => project.Status);
        builder.HasIndex(project => project.Priority);
        builder.HasIndex(project => project.DueDate);
    }
}
