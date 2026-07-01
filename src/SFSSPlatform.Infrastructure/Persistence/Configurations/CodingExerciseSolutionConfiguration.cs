using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence.Configurations;

public sealed class CodingExerciseSolutionConfiguration : IEntityTypeConfiguration<CodingExerciseSolution>
{
    public void Configure(EntityTypeBuilder<CodingExerciseSolution> builder)
    {
        builder.ToTable("CodingExerciseSolutions");
        builder.HasKey(solution => solution.Id);

        builder.Property(solution => solution.Code).IsRequired();

        builder.HasIndex(solution => solution.CodingExerciseId).IsUnique();

        builder.HasOne(solution => solution.CodingExercise)
            .WithMany()
            .HasForeignKey(solution => solution.CodingExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
