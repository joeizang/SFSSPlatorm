using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class TopicResourceLinkConfiguration : IEntityTypeConfiguration<TopicResourceLink>
{
    public void Configure(EntityTypeBuilder<TopicResourceLink> builder)
    {
        builder.ToTable("TopicResourceLinks");
        builder.HasKey(link => link.Id);

        builder.Property(link => link.Notes).HasMaxLength(2000).IsRequired();

        builder.HasIndex(link => link.TopicId);
        builder.HasIndex(link => link.LearningResourceId);
        builder.HasIndex(link => link.VideoCandidateId);
        builder.HasIndex(link => new { link.TopicId, link.LearningResourceId }).IsUnique();
        builder.HasIndex(link => new { link.TopicId, link.VideoCandidateId }).IsUnique();

        builder.HasOne(link => link.Topic)
            .WithMany()
            .HasForeignKey(link => link.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(link => link.LearningResource)
            .WithMany()
            .HasForeignKey(link => link.LearningResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(link => link.VideoCandidate)
            .WithMany()
            .HasForeignKey(link => link.VideoCandidateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
