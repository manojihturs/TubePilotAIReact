using Microsoft.EntityFrameworkCore;
using TubePilotAIReact.Server.Models;

namespace TubePilotAIReact.Server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<PromptCategory> PromptCategories { get; set; } = null!;
    public DbSet<Prompt> Prompts { get; set; } = null!;
    public DbSet<PromptVariable> PromptVariables { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PromptCategory
        modelBuilder.Entity<PromptCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });

        // Configure Prompt
        modelBuilder.Entity<Prompt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Prompts)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure PromptVariable
        modelBuilder.Entity<PromptVariable>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DefaultValue).HasMaxLength(500);
            entity.HasOne(e => e.Prompt)
                .WithMany(p => p.Variables)
                .HasForeignKey(e => e.PromptId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed sample data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categories
        modelBuilder.Entity<PromptCategory>().HasData(
            new PromptCategory { Id = 1, Name = "YouTube Video", Description = "Prompts for YouTube video content" },
            new PromptCategory { Id = 2, Name = "Instagram Reels", Description = "Prompts for Instagram short videos" },
            new PromptCategory { Id = 3, Name = "TikTok", Description = "Prompts for TikTok content" },
            new PromptCategory { Id = 4, Name = "Twitter/X", Description = "Prompts for Twitter posts" }
        );

        // Seed Prompts
        modelBuilder.Entity<Prompt>().HasData(
            new Prompt 
            { 
                Id = 1, 
                Title = "Video Title Generator", 
                Content = "Generate an engaging YouTube video title about {topic} that includes keywords for SEO.", 
                CategoryId = 1 
            },
            new Prompt 
            { 
                Id = 2, 
                Title = "Reel Caption Creator", 
                Content = "Create a catchy and engaging Instagram Reel caption for a video about {content_type} with emojis.", 
                CategoryId = 2 
            },
            new Prompt 
            { 
                Id = 3, 
                Title = "TikTok Script", 
                Content = "Write a short 30-second TikTok script about {topic} that's viral-worthy and trend-focused.", 
                CategoryId = 3 
            },
            new Prompt 
            { 
                Id = 4, 
                Title = "Tweet Creator", 
                Content = "Generate a Twitter post about {topic} with relevant hashtags and engaging call-to-action.", 
                CategoryId = 4 
            }
        );

        // Seed Variables
        modelBuilder.Entity<PromptVariable>().HasData(
            new PromptVariable { Id = 1, PromptId = 1, Name = "topic", Description = "The topic of the video", DefaultValue = "AI and Tech" },
            new PromptVariable { Id = 2, PromptId = 2, Name = "content_type", Description = "Type of content", DefaultValue = "Travel" },
            new PromptVariable { Id = 3, PromptId = 3, Name = "topic", Description = "Trending topic", DefaultValue = "Dance Challenge" },
            new PromptVariable { Id = 4, PromptId = 4, Name = "topic", Description = "Topic to tweet about", DefaultValue = "Technology News" }
        );
    }
}
