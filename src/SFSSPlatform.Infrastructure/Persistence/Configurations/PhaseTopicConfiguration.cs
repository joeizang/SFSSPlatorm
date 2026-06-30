using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.Curriculum;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class PhaseTopicConfiguration : IEntityTypeConfiguration<PhaseTopic>
{
    public void Configure(EntityTypeBuilder<PhaseTopic> builder)
    {
        builder.ToTable("PhaseTopics");
        builder.HasKey(phaseTopic => new { phaseTopic.PhaseId, phaseTopic.TopicId });

        builder.HasOne(phaseTopic => phaseTopic.Phase)
            .WithMany(phase => phase.Topics)
            .HasForeignKey(phaseTopic => phaseTopic.PhaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(phaseTopic => phaseTopic.Topic)
            .WithMany()
            .HasForeignKey(phaseTopic => phaseTopic.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(phaseTopic => new { phaseTopic.PhaseId, phaseTopic.Order });
    }
}
