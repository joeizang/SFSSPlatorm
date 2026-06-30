using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class TrustedYouTubeChannelConfiguration : IEntityTypeConfiguration<TrustedYouTubeChannel>
{
    public void Configure(EntityTypeBuilder<TrustedYouTubeChannel> builder)
    {
        builder.ToTable("TrustedYouTubeChannels");
        builder.HasKey(channel => channel.Id);

        builder.Property(channel => channel.Name).HasMaxLength(180).IsRequired();
        builder.Property(channel => channel.Url).HasMaxLength(800).IsRequired();
        builder.Property(channel => channel.Tags).HasMaxLength(1000).IsRequired();
        builder.Property(channel => channel.Priority).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(channel => channel.Notes).HasMaxLength(2000).IsRequired();

        builder.HasIndex(channel => channel.Url).IsUnique();
        builder.HasIndex(channel => channel.Priority);
        builder.HasIndex(channel => channel.Tags);
    }
}
