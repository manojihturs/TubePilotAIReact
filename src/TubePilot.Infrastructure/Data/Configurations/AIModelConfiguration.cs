using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class AIModelConfiguration : IEntityTypeConfiguration<AIModel>
    {
        public void Configure(EntityTypeBuilder<AIModel> builder)
        {
            builder.ToTable("AIModels");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.DisplayName).HasMaxLength(250);

            builder.HasIndex(x => new { x.ProviderId, x.Name }).IsUnique();

            builder.Property(x => x.ContextWindow);
            builder.Property(x => x.MaxInputTokens);
            builder.Property(x => x.MaxOutputTokens);

            builder.Property(x => x.InputPrice).HasColumnType("decimal(18,8)");
            builder.Property(x => x.OutputPrice).HasColumnType("decimal(18,8)");

            builder.Property(x => x.IsEnabled).HasDefaultValue(true);
            builder.Property(x => x.CreatedDate).IsRequired();

            builder.HasOne(x => x.Provider).WithMany().HasForeignKey(x => x.ProviderId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
