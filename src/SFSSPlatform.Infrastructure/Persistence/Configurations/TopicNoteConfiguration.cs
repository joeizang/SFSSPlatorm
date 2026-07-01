using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.Curriculum;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class TopicNoteConfiguration : IEntityTypeConfiguration<TopicNote>
{
    public void Configure(EntityTypeBuilder<TopicNote> builder)
    {
        builder.ToTable("TopicNotes");
        builder.HasKey(note => note.Id);

        builder.Property(note => note.Content).HasMaxLength(40_000).IsRequired();

        builder.HasIndex(note => note.TopicId).IsUnique();
        builder.HasOne(note => note.Topic)
            .WithOne()
            .HasForeignKey<TopicNote>(note => note.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
