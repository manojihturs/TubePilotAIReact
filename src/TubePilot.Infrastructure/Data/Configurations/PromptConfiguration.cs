using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class PromptConfiguration : IEntityTypeConfiguration<Prompt>
    {
        public void Configure(EntityTypeBuilder<Prompt> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.PromptText).IsRequired();
            builder.Property(x => x.Model).HasMaxLength(200);
            builder.Property(x => x.Provider).HasMaxLength(200);
            builder.Property(x => x.Version).HasMaxLength(100);
            builder.Property(x => x.Status).HasMaxLength(50);
            builder.Property(x => x.Tags).HasMaxLength(500);
            builder.Property(x => x.CreatedBy).HasMaxLength(200);
            builder.Property(x => x.UpdatedBy).HasMaxLength(200);

            builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Name);
            builder.HasIndex(x => x.CategoryId);
        }
    }
}
