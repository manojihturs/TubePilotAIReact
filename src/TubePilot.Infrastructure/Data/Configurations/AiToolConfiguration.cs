using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class AiToolConfiguration : IEntityTypeConfiguration<AiTool>
    {
        public void Configure(EntityTypeBuilder<AiTool> builder)
        {
            builder.ToTable("AiTools");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ApiFormat).IsRequired().HasMaxLength(50);
            builder.Property(x => x.BaseUrl).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Model).IsRequired().HasMaxLength(200);
            builder.Property(x => x.EncryptedApiKey).IsRequired().HasMaxLength(4000);
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasIndex(x => new { x.UserId, x.Priority });
        }
    }
}
