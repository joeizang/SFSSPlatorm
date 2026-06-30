using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.Curriculum;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("Topics");
        builder.HasKey(topic => topic.Id);

        builder.Property(topic => topic.ExternalId).HasMaxLength(160).IsRequired();
        builder.Property(topic => topic.Slug).HasMaxLength(200).IsRequired();
        builder.Property(topic => topic.Title).HasMaxLength(280).IsRequired();
        builder.Property(topic => topic.Summary).HasMaxLength(2_000);

        builder.OwnsOne(topic => topic.Progress, progress =>
        {
            progress.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(40)
                .HasColumnName("ProgressStatus");
            progress.Property(p => p.UpdatedAt).HasColumnName("ProgressUpdatedAt");
            progress.Property(p => p.StartedAt).HasColumnName("ProgressStartedAt");
            progress.Property(p => p.CompletedAt).HasColumnName("ProgressCompletedAt");
        });

        builder.HasIndex(topic => topic.ExternalId).IsUnique();
        builder.HasIndex(topic => topic.Slug).IsUnique();
        builder.HasIndex(topic => new { topic.ModuleId, topic.Order });
        builder.Navigation(topic => topic.TaskTypes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
