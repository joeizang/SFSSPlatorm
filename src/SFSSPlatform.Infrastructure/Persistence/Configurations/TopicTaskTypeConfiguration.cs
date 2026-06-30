using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.Curriculum;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class TopicTaskTypeConfiguration : IEntityTypeConfiguration<TopicTaskType>
{
    public void Configure(EntityTypeBuilder<TopicTaskType> builder)
    {
        builder.ToTable("TopicTaskTypes");
        builder.HasKey(topicTaskType => new { topicTaskType.TopicId, topicTaskType.TaskType });

        builder.Property(topicTaskType => topicTaskType.TaskType)
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.HasOne(topicTaskType => topicTaskType.Topic)
            .WithMany(topic => topic.TaskTypes)
            .HasForeignKey(topicTaskType => topicTaskType.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
