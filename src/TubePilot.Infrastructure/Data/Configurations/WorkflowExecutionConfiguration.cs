using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data.Configurations
{
    public class WorkflowExecutionConfiguration : IEntityTypeConfiguration<WorkflowExecution>
    {
        public void Configure(EntityTypeBuilder<WorkflowExecution> builder)
        {
            builder.ToTable("WorkflowExecutions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.InputJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.ResultJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.StartedAt).IsRequired();
        }
    }
}
