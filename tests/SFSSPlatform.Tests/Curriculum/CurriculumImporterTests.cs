using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.Curriculum;
using SFSSPlatform.Infrastructure.Curriculum;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Tests.Curriculum;

public sealed class CurriculumImporterTests
{
    [Fact]
    public async Task Reimport_updates_reference_data_without_destroying_progress()
    {
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        var importer = new CurriculumImporter(db);
        await importer.ImportAsync(TestCurriculumSeed.Create());

        var topic = await db.Topics.SingleAsync(t => t.ExternalId == "topic-minimal-api-routing");
        topic.SetProgress(TopicProgressStatus.Done, DateTimeOffset.Parse("2026-06-30T12:00:00Z"));
        await db.SaveChangesAsync();

        var changedSeed = new CurriculumSeed(
            [
                TestCurriculumSeed.Create().Modules.First() with
                {
                    Topics =
                    [
                        TestCurriculumSeed.Create().Modules.First().Topics.First() with
                        {
                            Title = "Minimal API route groups"
                        }
                    ]
                }
            ],
            [
                new PhaseSeed(
                    "phase-one",
                    "phase-one",
                    "Phase One",
                    "Foundation path.",
                    1,
                    [new PhaseTopicSeed("topic-minimal-api-routing", 1)])
            ]);

        await importer.ImportAsync(changedSeed);

        var savedTopic = await db.Topics.SingleAsync(t => t.ExternalId == "topic-minimal-api-routing");
        Assert.Equal("Minimal API route groups", savedTopic.Title);
        Assert.Equal(TopicProgressStatus.Done, savedTopic.Progress.Status);
        Assert.Equal(1, await db.Topics.CountAsync(t => t.ExternalId == "topic-minimal-api-routing"));
    }

    private static StudyPlatformDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<StudyPlatformDbContext>()
            .UseSqlite($"Data Source={Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db")}")
            .Options;

        return new StudyPlatformDbContext(options);
    }
}
