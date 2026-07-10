using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class AIProviderConfiguration : IEntityTypeConfiguration<AIProvider>
    {
        public void Configure(EntityTypeBuilder<AIProvider> builder)
        {
            builder.ToTable("AIProviders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(x => x.Name).IsUnique();

            builder.Property(x => x.DisplayName).HasMaxLength(250);

            builder.Property(x => x.ProviderType).IsRequired().HasMaxLength(100);

            builder.Property(x => x.BaseUrl).HasMaxLength(1000);

            builder.Property(x => x.ApiKeyEncrypted).HasMaxLength(2000);

            builder.Property(x => x.DefaultModel).HasMaxLength(200);

            builder.Property(x => x.Priority).HasDefaultValue(0);

            builder.Property(x => x.IsEnabled).HasDefaultValue(true);

            builder.Property(x => x.CreatedDate).IsRequired();
        }
    }
}
