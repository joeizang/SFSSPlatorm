using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class CodingExerciseConfiguration : IEntityTypeConfiguration<CodingExercise>
{
    public void Configure(EntityTypeBuilder<CodingExercise> builder)
    {
        builder.ToTable("CodingExercises");
        builder.HasKey(exercise => exercise.Id);

        builder.Property(exercise => exercise.Title).HasMaxLength(280).IsRequired();
        builder.Property(exercise => exercise.Prompt).IsRequired();
        builder.Property(exercise => exercise.Difficulty).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(exercise => exercise.Language).HasMaxLength(80).IsRequired();
        builder.Property(exercise => exercise.StarterCode).IsRequired();
        builder.Property(exercise => exercise.PackageRequirements).IsRequired();
        builder.Property(exercise => exercise.SuccessCriteria).IsRequired();
        builder.Property(exercise => exercise.Hints).IsRequired();
        builder.Property(exercise => exercise.CheckDefinitionJson).IsRequired();

        builder.HasIndex(exercise => new { exercise.TopicId, exercise.Title }).IsUnique();

        builder.HasOne(exercise => exercise.Topic)
            .WithMany()
            .HasForeignKey(exercise => exercise.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
