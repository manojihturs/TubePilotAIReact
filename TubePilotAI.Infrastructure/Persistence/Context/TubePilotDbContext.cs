using Microsoft.EntityFrameworkCore;
using TubePilotAI.Domain.Entities;

namespace TubePilotAI.Infrastructure.Persistence.Context;

public sealed class TubePilotDbContext(DbContextOptions<TubePilotDbContext> options) : DbContext(options)
{
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<GeneratedContent> GeneratedContents => Set<GeneratedContent>();

    public DbSet<ContentGenerationJob> ContentGenerationJobs => Set<ContentGenerationJob>();

    public DbSet<AIProviderSetting> AIProviderSettings => Set<AIProviderSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TubePilotDbContext).Assembly);
    }
}
