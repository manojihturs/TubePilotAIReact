using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class ProjectOutputConfiguration : IEntityTypeConfiguration<ProjectOutput>
    {
        public void Configure(EntityTypeBuilder<ProjectOutput> builder)
        {
            builder.ToTable("ProjectOutputs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OutputItemName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.FolderName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.FilePath).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.HasIndex(x => x.ProjectId);
        }
    }
}
