using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.Curriculum;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class PhaseConfiguration : IEntityTypeConfiguration<Phase>
{
    public void Configure(EntityTypeBuilder<Phase> builder)
    {
        builder.ToTable("Phases");
        builder.HasKey(phase => phase.Id);

        builder.Property(phase => phase.ExternalId).HasMaxLength(120).IsRequired();
        builder.Property(phase => phase.Slug).HasMaxLength(160).IsRequired();
        builder.Property(phase => phase.Title).HasMaxLength(240).IsRequired();
        builder.Property(phase => phase.Description).HasMaxLength(2_000);

        builder.HasIndex(phase => phase.ExternalId).IsUnique();
        builder.HasIndex(phase => phase.Slug).IsUnique();
        builder.Navigation(phase => phase.Topics).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
