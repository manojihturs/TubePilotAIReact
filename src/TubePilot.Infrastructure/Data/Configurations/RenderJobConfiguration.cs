using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class RenderJobConfiguration : IEntityTypeConfiguration<RenderJob>
    {
        public void Configure(EntityTypeBuilder<RenderJob> builder)
        {
            builder.ToTable("RenderJobs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Language).IsRequired().HasMaxLength(10);
            builder.Property(x => x.OutputPath).HasMaxLength(1000);
            builder.Property(x => x.ErrorMessage).HasColumnType("text");
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.HasIndex(x => x.ProjectId);
        }
    }
}
