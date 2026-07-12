using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class PromptVersionConfiguration : IEntityTypeConfiguration<PromptVersion>
    {
        public void Configure(EntityTypeBuilder<PromptVersion> builder)
        {
            builder.ToTable("PromptVersions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TemplateText).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasIndex(x => new { x.PromptId, x.VersionNumber }).IsUnique();
        }
    }
}
