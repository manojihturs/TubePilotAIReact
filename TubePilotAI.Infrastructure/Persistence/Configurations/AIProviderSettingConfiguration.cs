using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilotAI.Domain.Entities;

namespace TubePilotAI.Infrastructure.Persistence.Configurations;

public sealed class AIProviderSettingConfiguration : IEntityTypeConfiguration<AIProviderSetting>
{
    public void Configure(EntityTypeBuilder<AIProviderSetting> builder)
    {
        builder.ToTable("AIProviderSettings");

        builder.HasKey(setting => setting.Id);

        builder.Property(setting => setting.Provider)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(setting => setting.ApiKey)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(setting => setting.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(setting => setting.UpdatedAtUtc)
            .HasColumnType("datetime2");

        builder.HasIndex(setting => setting.Provider)
            .IsUnique();
    }
}
