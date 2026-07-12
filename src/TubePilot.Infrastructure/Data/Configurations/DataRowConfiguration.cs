using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class DataRowConfiguration : IEntityTypeConfiguration<DataRow>
    {
        public void Configure(EntityTypeBuilder<DataRow> builder)
        {
            builder.ToTable("DataRows");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DataJson).IsRequired().HasColumnType("text");
            builder.Property(x => x.ConfirmedImagePath).HasMaxLength(1000);
            builder.HasIndex(x => x.ProjectOutputId);
        }
    }
}
