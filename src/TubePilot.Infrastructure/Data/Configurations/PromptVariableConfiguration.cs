using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class PromptVariableConfiguration : IEntityTypeConfiguration<PromptVariable>
    {
        public void Configure(EntityTypeBuilder<PromptVariable> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Placeholder).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.DataType).HasMaxLength(50);
            builder.Property(x => x.DefaultValue).HasMaxLength(200);
            builder.Property(x => x.CreatedBy).HasMaxLength(200);
            builder.Property(x => x.UpdatedBy).HasMaxLength(200);

            builder.HasOne(x => x.Prompt).WithMany().HasForeignKey(x => x.PromptId).OnDelete(DeleteBehavior.Cascade);

            // Unique per prompt by name
            builder.HasIndex(x => new { x.PromptId, x.Name }).IsUnique();
        }
    }
}
