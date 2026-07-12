using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class GenerationRecordConfiguration : IEntityTypeConfiguration<GenerationRecord>
    {
        public void Configure(EntityTypeBuilder<GenerationRecord> builder)
        {
            builder.ToTable("GenerationRecords");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OutputItemName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ProviderUsed).IsRequired().HasMaxLength(100);
            builder.Property(x => x.ModelUsed).IsRequired().HasMaxLength(200);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.HasIndex(x => x.ProjectId);
        }
    }
}
