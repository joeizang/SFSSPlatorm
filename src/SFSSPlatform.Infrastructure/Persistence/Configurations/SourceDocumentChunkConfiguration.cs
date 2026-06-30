using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class SourceDocumentChunkConfiguration : IEntityTypeConfiguration<SourceDocumentChunk>
{
    public void Configure(EntityTypeBuilder<SourceDocumentChunk> builder)
    {
        builder.ToTable("SourceDocumentChunks");
        builder.HasKey(chunk => chunk.Id);

        builder.Property(chunk => chunk.Heading).HasMaxLength(500).IsRequired();
        builder.Property(chunk => chunk.Text).IsRequired();

        builder.HasIndex(chunk => new { chunk.SourceMaterialId, chunk.Order }).IsUnique();
        builder.HasIndex(chunk => new { chunk.SourceMaterialId, chunk.StartPage });

        builder.HasOne(chunk => chunk.SourceMaterial)
            .WithMany(source => source.Chunks)
            .HasForeignKey(chunk => chunk.SourceMaterialId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
