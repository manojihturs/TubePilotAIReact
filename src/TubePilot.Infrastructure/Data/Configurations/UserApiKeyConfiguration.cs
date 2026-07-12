using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class UserApiKeyConfiguration : IEntityTypeConfiguration<UserApiKey>
    {
        public void Configure(EntityTypeBuilder<UserApiKey> builder)
        {
            builder.ToTable("UserApiKeys");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProviderName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.EncryptedKey)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasIndex(x => new { x.UserId, x.ProviderName }).IsUnique();
        }
    }
}
