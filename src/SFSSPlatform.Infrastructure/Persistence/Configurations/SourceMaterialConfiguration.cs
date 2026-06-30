using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class SourceMaterialConfiguration : IEntityTypeConfiguration<SourceMaterial>
{
    public void Configure(EntityTypeBuilder<SourceMaterial> builder)
    {
        builder.ToTable("SourceMaterials");
        builder.HasKey(source => source.Id);

        builder.Property(source => source.StableKey).HasMaxLength(160).IsRequired();
        builder.Property(source => source.Title).HasMaxLength(500).IsRequired();
        builder.Property(source => source.Author).HasMaxLength(300);
        builder.Property(source => source.FileName).HasMaxLength(600).IsRequired();
        builder.Property(source => source.RelativePath).HasMaxLength(1_000).IsRequired();
        builder.Property(source => source.Access).HasConversion<string>().HasMaxLength(60);
        builder.Property(source => source.ExtractionStatus).HasConversion<string>().HasMaxLength(60);
        builder.Property(source => source.ExtractionError).HasMaxLength(2_000);

        builder.HasIndex(source => source.StableKey).IsUnique();
        builder.HasIndex(source => source.FileName);
        builder.Navigation(source => source.Chunks).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
