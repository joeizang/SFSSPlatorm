using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class LearningResourceConfiguration : IEntityTypeConfiguration<LearningResource>
{
    public void Configure(EntityTypeBuilder<LearningResource> builder)
    {
        builder.ToTable("LearningResources");
        builder.HasKey(resource => resource.Id);

        builder.Property(resource => resource.ExternalId).HasMaxLength(120).IsRequired();
        builder.Property(resource => resource.Provider).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(resource => resource.Title).HasMaxLength(320).IsRequired();
        builder.Property(resource => resource.Creator).HasMaxLength(180).IsRequired();
        builder.Property(resource => resource.Url).HasMaxLength(800).IsRequired();
        builder.Property(resource => resource.EmbedUrl).HasMaxLength(800).IsRequired();
        builder.Property(resource => resource.Summary).HasMaxLength(2000).IsRequired();
        builder.Property(resource => resource.Tags).HasMaxLength(1000).IsRequired();
        builder.Property(resource => resource.Notes).HasMaxLength(8000).IsRequired();

        builder.HasIndex(resource => new { resource.Provider, resource.ExternalId }).IsUnique();
        builder.HasIndex(resource => resource.IsWatched);
        builder.HasIndex(resource => resource.Tags);
        builder.HasIndex(resource => resource.SourceDocumentChunkId);

        builder.HasOne(resource => resource.SourceMaterial)
            .WithMany()
            .HasForeignKey(resource => resource.SourceMaterialId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(resource => resource.SourceDocumentChunk)
            .WithMany()
            .HasForeignKey(resource => resource.SourceDocumentChunkId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
