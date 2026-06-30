using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class VideoCandidateConfiguration : IEntityTypeConfiguration<VideoCandidate>
{
    public void Configure(EntityTypeBuilder<VideoCandidate> builder)
    {
        builder.ToTable("VideoCandidates");
        builder.HasKey(candidate => candidate.Id);

        builder.Property(candidate => candidate.ExternalId).HasMaxLength(120).IsRequired();
        builder.Property(candidate => candidate.Title).HasMaxLength(320).IsRequired();
        builder.Property(candidate => candidate.ChannelName).HasMaxLength(180).IsRequired();
        builder.Property(candidate => candidate.ChannelUrl).HasMaxLength(800).IsRequired();
        builder.Property(candidate => candidate.Url).HasMaxLength(800).IsRequired();
        builder.Property(candidate => candidate.EmbedUrl).HasMaxLength(800).IsRequired();
        builder.Property(candidate => candidate.Summary).HasMaxLength(2000).IsRequired();
        builder.Property(candidate => candidate.Tags).HasMaxLength(1000).IsRequired();
        builder.Property(candidate => candidate.Difficulty).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(candidate => candidate.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(candidate => candidate.Notes).HasMaxLength(4000).IsRequired();
        builder.Property(candidate => candidate.RejectionReason).HasMaxLength(2000).IsRequired();

        builder.HasIndex(candidate => candidate.ExternalId).IsUnique();
        builder.HasIndex(candidate => candidate.ChannelName);
        builder.HasIndex(candidate => candidate.Difficulty);
        builder.HasIndex(candidate => candidate.Status);
        builder.HasIndex(candidate => candidate.Tags);

        builder.HasOne(candidate => candidate.LearningResource)
            .WithMany()
            .HasForeignKey(candidate => candidate.LearningResourceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
