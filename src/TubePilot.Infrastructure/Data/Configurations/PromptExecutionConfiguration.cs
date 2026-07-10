using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class PromptExecutionConfiguration : IEntityTypeConfiguration<PromptExecution>
    {
        public void Configure(EntityTypeBuilder<PromptExecution> builder)
        {
            builder.ToTable("PromptExecutions");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.VariablesJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.RenderedPrompt).HasColumnType("nvarchar(max)");
            builder.Property(x => x.Response).HasColumnType("nvarchar(max)");

            builder.Property(x => x.EstimatedCost).HasColumnType("decimal(18,8)");

            builder.HasOne(x => x.Prompt).WithMany().HasForeignKey(x => x.PromptId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Provider).WithMany().HasForeignKey(x => x.ProviderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Model).WithMany().HasForeignKey(x => x.ModelId).OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.CreatedDate).IsRequired();
        }
    }
}
