using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class StudyItemConfiguration : IEntityTypeConfiguration<StudyItem>
{
    public void Configure(EntityTypeBuilder<StudyItem> builder)
    {
        builder.ToTable("StudyItems");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Kind).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(item => item.Prompt).HasMaxLength(2000).IsRequired();
        builder.Property(item => item.ExpectedAnswer).IsRequired();
        builder.Property(item => item.Explanation).IsRequired();
        builder.Property(item => item.SourceExcerpt).IsRequired();

        builder.HasIndex(item => new { item.SourceDocumentChunkId, item.Kind });
        builder.HasIndex(item => item.Status);
        builder.HasIndex(item => new { item.Status, item.NextReviewAt, item.Id });

        builder.HasOne(item => item.SourceMaterial)
            .WithMany()
            .HasForeignKey(item => item.SourceMaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.SourceDocumentChunk)
            .WithMany()
            .HasForeignKey(item => item.SourceDocumentChunkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
