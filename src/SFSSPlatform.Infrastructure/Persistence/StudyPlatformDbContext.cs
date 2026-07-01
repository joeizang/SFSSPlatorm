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

    public DbSet<TopicNote> TopicNotes => Set<TopicNote>();

    public DbSet<TopicTaskType> TopicTaskTypes => Set<TopicTaskType>();

    public DbSet<SourceMaterial> SourceMaterials => Set<SourceMaterial>();

    public DbSet<SourceDocumentChunk> SourceDocumentChunks => Set<SourceDocumentChunk>();

    public DbSet<StudyItem> StudyItems => Set<StudyItem>();

    public DbSet<CodingExercise> CodingExercises => Set<CodingExercise>();

    public DbSet<CodingExerciseSolution> CodingExerciseSolutions => Set<CodingExerciseSolution>();

    public DbSet<StudyAttempt> StudyAttempts => Set<StudyAttempt>();

    public DbSet<LearningResource> LearningResources => Set<LearningResource>();

    public DbSet<TrustedYouTubeChannel> TrustedYouTubeChannels => Set<TrustedYouTubeChannel>();

    public DbSet<VideoCandidate> VideoCandidates => Set<VideoCandidate>();

    public DbSet<TopicResourceLink> TopicResourceLinks => Set<TopicResourceLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudyPlatformDbContext).Assembly);
    }
}
