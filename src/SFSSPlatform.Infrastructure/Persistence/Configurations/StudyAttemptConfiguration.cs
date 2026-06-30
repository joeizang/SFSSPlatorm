using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class StudyAttemptConfiguration : IEntityTypeConfiguration<StudyAttempt>
{
    public void Configure(EntityTypeBuilder<StudyAttempt> builder)
    {
        builder.ToTable("StudyAttempts");
        builder.HasKey(attempt => attempt.Id);

        builder.Property(attempt => attempt.Answer).IsRequired();
        builder.Property(attempt => attempt.Rating).HasConversion<string>().HasMaxLength(40).IsRequired();

        builder.HasIndex(attempt => new { attempt.StudyItemId, attempt.AttemptedAt });

        builder.HasOne(attempt => attempt.StudyItem)
            .WithMany()
            .HasForeignKey(attempt => attempt.StudyItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
