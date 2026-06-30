using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.Curriculum;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence;

public sealed class StudyPlatformDbContext(DbContextOptions<StudyPlatformDbContext> options)
    : DbContext(options)
{
    public DbSet<CurriculumModule> Modules => Set<CurriculumModule>();

    public DbSet<Phase> Phases => Set<Phase>();

    public DbSet<PhaseTopic> PhaseTopics => Set<PhaseTopic>();

    public DbSet<Topic> Topics => Set<Topic>();

    public DbSet<TopicTaskType> TopicTaskTypes => Set<TopicTaskType>();

    public DbSet<SourceMaterial> SourceMaterials => Set<SourceMaterial>();

    public DbSet<SourceDocumentChunk> SourceDocumentChunks => Set<SourceDocumentChunk>();

    public DbSet<StudyItem> StudyItems => Set<StudyItem>();

    public DbSet<StudyAttempt> StudyAttempts => Set<StudyAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudyPlatformDbContext).Assembly);
    }
}
