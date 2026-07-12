using Microsoft.EntityFrameworkCore;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data
{
    public class TubePilotDbContext : DbContext
    {
        public TubePilotDbContext(DbContextOptions<TubePilotDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<TubePilot.Domain.Entities.PromptCategory> PromptCategories => Set<TubePilot.Domain.Entities.PromptCategory>();
        public DbSet<TubePilot.Domain.Entities.Prompt> Prompts => Set<TubePilot.Domain.Entities.Prompt>();
        public DbSet<TubePilot.Domain.Entities.PromptVariable> PromptVariables => Set<TubePilot.Domain.Entities.PromptVariable>();
        public DbSet<TubePilot.Domain.Entities.UserApiKey> UserApiKeys => Set<TubePilot.Domain.Entities.UserApiKey>();
        public DbSet<TubePilot.Domain.Entities.Project> Projects => Set<TubePilot.Domain.Entities.Project>();
        public DbSet<TubePilot.Domain.Entities.PromptVersion> PromptVersions => Set<TubePilot.Domain.Entities.PromptVersion>();
        public DbSet<TubePilot.Domain.Entities.ProjectOutput> ProjectOutputs => Set<TubePilot.Domain.Entities.ProjectOutput>();
        public DbSet<TubePilot.Domain.Entities.DataRow> DataRows => Set<TubePilot.Domain.Entities.DataRow>();
        public DbSet<TubePilot.Domain.Entities.GenerationRecord> GenerationRecords => Set<TubePilot.Domain.Entities.GenerationRecord>();
        public DbSet<TubePilot.Domain.Entities.RenderJob> RenderJobs => Set<TubePilot.Domain.Entities.RenderJob>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Email).IsRequired();
                b.Property(x => x.Password).IsRequired();
                b.Property(x => x.Role).HasMaxLength(64);
            });

            // Apply PromptCategory configuration
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.PromptCategoryConfiguration());

            // Apply Prompt configuration
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.PromptConfiguration());
            // Apply PromptVariable configuration
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.PromptVariableConfiguration());
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.UserApiKeyConfiguration());
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.ProjectConfiguration());
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.PromptVersionConfiguration());
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.ProjectOutputConfiguration());
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.DataRowConfiguration());
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.GenerationRecordConfiguration());
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.RenderJobConfiguration());
        }
    }
}
