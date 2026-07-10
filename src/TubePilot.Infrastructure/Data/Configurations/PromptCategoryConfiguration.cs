using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class PromptCategoryConfiguration : IEntityTypeConfiguration<PromptCategory>
    {
        public void Configure(EntityTypeBuilder<PromptCategory> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Icon).HasMaxLength(200);
            builder.Property(x => x.Color).HasMaxLength(50);
            builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.SoftDelete).HasDefaultValue(false);
            builder.Property(x => x.CreatedDate).IsRequired();
            builder.Property(x => x.CreatedBy).HasMaxLength(200);
            builder.Property(x => x.UpdatedBy).HasMaxLength(200);

            // Unique index on Name
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
