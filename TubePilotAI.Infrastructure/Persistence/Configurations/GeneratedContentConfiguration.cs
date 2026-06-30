using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TubePilotAI.Domain.Entities;

namespace TubePilotAI.Infrastructure.Persistence.Configurations;

public sealed class GeneratedContentConfiguration : IEntityTypeConfiguration<GeneratedContent>
{
    public void Configure(EntityTypeBuilder<GeneratedContent> builder)
    {
        builder.ToTable("GeneratedContent");

        builder.HasKey(gc => gc.Id);

        builder.HasOne(gc => gc.Project)
            .WithMany(p => p.GeneratedContents)
            .HasForeignKey(gc => gc.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(gc => gc.Title).HasMaxLength(256).IsRequired();
        builder.Property(gc => gc.Description).HasMaxLength(2000).IsRequired();
        builder.Property(gc => gc.Hashtags).HasMaxLength(500).IsRequired();
        builder.Property(gc => gc.ThumbnailText).HasMaxLength(500).IsRequired();
        builder.Property(gc => gc.ThumbnailPrompt).HasMaxLength(1000).IsRequired();
        builder.Property(gc => gc.NarrationScript).IsRequired();
        builder.Property(gc => gc.SceneBreakdown).IsRequired();
        builder.Property(gc => gc.VoiceoverScript).IsRequired();
        
        builder.Property(gc => gc.CreatedAtUtc).IsRequired();
    }
}
