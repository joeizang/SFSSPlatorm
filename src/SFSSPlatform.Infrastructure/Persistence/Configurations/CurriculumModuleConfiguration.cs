using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.Curriculum;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class CurriculumModuleConfiguration : IEntityTypeConfiguration<CurriculumModule>
{
    public void Configure(EntityTypeBuilder<CurriculumModule> builder)
    {
        builder.ToTable("Modules");
        builder.HasKey(module => module.Id);

        builder.Property(module => module.ExternalId).HasMaxLength(120).IsRequired();
        builder.Property(module => module.Slug).HasMaxLength(160).IsRequired();
        builder.Property(module => module.Title).HasMaxLength(240).IsRequired();
        builder.Property(module => module.Description).HasMaxLength(2_000);

        builder.HasIndex(module => module.ExternalId).IsUnique();
        builder.HasIndex(module => module.Slug).IsUnique();
        builder.HasMany(module => module.Topics)
            .WithOne(topic => topic.Module)
            .HasForeignKey(topic => topic.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(module => module.Topics).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
