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
        public DbSet<TubePilot.Domain.Entities.AIProvider> AIProviders => Set<TubePilot.Domain.Entities.AIProvider>();
        public DbSet<TubePilot.Domain.Entities.AIModel> AIModels => Set<TubePilot.Domain.Entities.AIModel>();
        public DbSet<TubePilot.Domain.Entities.PromptExecution> PromptExecutions => Set<TubePilot.Domain.Entities.PromptExecution>();
        public DbSet<TubePilot.Domain.Entities.Workflow> Workflows => Set<TubePilot.Domain.Entities.Workflow>();
        public DbSet<TubePilot.Domain.Entities.WorkflowStep> WorkflowSteps => Set<TubePilot.Domain.Entities.WorkflowStep>();
        public DbSet<TubePilot.Domain.Entities.WorkflowExecution> WorkflowExecutions => Set<TubePilot.Domain.Entities.WorkflowExecution>();

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
            // Apply AIProvider configuration
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.AIProviderConfiguration());
            // Apply AIModel configuration
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.AIModelConfiguration());
            // Apply PromptExecution configuration
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.PromptExecutionConfiguration());
            // Apply Workflow configurations
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.WorkflowConfiguration());
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.WorkflowStepConfiguration());
            modelBuilder.ApplyConfiguration(new TubePilot.Infrastructure.Data.Configurations.WorkflowExecutionConfiguration());
        }
    }
}