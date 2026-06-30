using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.Curriculum;
using SFSSPlatform.Infrastructure.Persistence;
using TaskType = SFSSPlatform.Domain.Curriculum.TaskType;

namespace SFSSPlatform.Infrastructure.Curriculum;

public sealed class CurriculumImporter(StudyPlatformDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task<CurriculumImportResult> ImportAsync(Stream seedJson, CancellationToken cancellationToken)
    {
        var seed = await JsonSerializer.DeserializeAsync<CurriculumSeed>(seedJson, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Curriculum seed file is empty or invalid.");

        return await ImportAsync(seed, cancellationToken);
    }

    public async Task<CurriculumImportResult> ImportAsync(
        CurriculumSeed seed,
        CancellationToken cancellationToken = default)
    {
        var moduleCount = 0;
        var topicCount = 0;
        var phaseCount = 0;
        var phaseTopicCount = 0;

        foreach (var moduleSeed in seed.Modules.OrEmpty().OrderBy(m => m.Order))
        {
            var moduleExternalId = Required(moduleSeed.Id, "Module id");
            var module = await db.Modules
                .SingleOrDefaultAsync(m => m.ExternalId == moduleExternalId, cancellationToken);

            if (module is null)
            {
                module = new CurriculumModule(
                    moduleExternalId,
                    SlugOrFallback(moduleSeed.Slug, moduleSeed.Title),
                    Required(moduleSeed.Title, "Module title"),
                    moduleSeed.Order,
                    moduleSeed.Description);
                db.Modules.Add(module);
            }
            else
            {
                module.Sync(
                    SlugOrFallback(moduleSeed.Slug, moduleSeed.Title),
                    Required(moduleSeed.Title, "Module title"),
                    moduleSeed.Order,
                    moduleSeed.Description);
            }

            await db.SaveChangesAsync(cancellationToken);
            moduleCount++;

            foreach (var topicSeed in moduleSeed.Topics.OrEmpty().OrderBy(t => t.Order))
            {
                var topicExternalId = Required(topicSeed.Id, "Topic id");
                var topic = await db.Topics
                    .SingleOrDefaultAsync(t => t.ExternalId == topicExternalId, cancellationToken);

                if (topic is null)
                {
                    topic = new Topic(
                        topicExternalId,
                        SlugOrFallback(topicSeed.Slug, topicSeed.Title),
                        Required(topicSeed.Title, "Topic title"),
                        module.Id,
                        topicSeed.Order,
                        topicSeed.Summary);
                    db.Topics.Add(topic);
                }
                else
                {
                    topic.Sync(
                        SlugOrFallback(topicSeed.Slug, topicSeed.Title),
                        Required(topicSeed.Title, "Topic title"),
                        module.Id,
                        topicSeed.Order,
                        topicSeed.Summary);
                }

                await db.SaveChangesAsync(cancellationToken);
                await SyncTaskTypesAsync(topic.Id, topicSeed.TaskTypes, cancellationToken);
                topicCount++;
            }
        }

        foreach (var phaseSeed in seed.Phases.OrEmpty().OrderBy(p => p.Order))
        {
            var phaseExternalId = Required(phaseSeed.Id, "Phase id");
            var phase = await db.Phases
                .SingleOrDefaultAsync(p => p.ExternalId == phaseExternalId, cancellationToken);

            if (phase is null)
            {
                phase = new Phase(
                    phaseExternalId,
                    SlugOrFallback(phaseSeed.Slug, phaseSeed.Title),
                    Required(phaseSeed.Title, "Phase title"),
                    phaseSeed.Order,
                    phaseSeed.Description);
                db.Phases.Add(phase);
            }
            else
            {
                phase.Sync(
                    SlugOrFallback(phaseSeed.Slug, phaseSeed.Title),
                    Required(phaseSeed.Title, "Phase title"),
                    phaseSeed.Order,
                    phaseSeed.Description);
            }

            await db.SaveChangesAsync(cancellationToken);
            phaseCount++;

            foreach (var phaseTopicSeed in phaseSeed.Topics.OrEmpty().OrderBy(t => t.Order))
            {
                var topicExternalId = Required(phaseTopicSeed.TopicId, "Phase topic id");
                var topicId = await db.Topics
                    .Where(t => t.ExternalId == topicExternalId)
                    .Select(t => t.Id)
                    .SingleAsync(cancellationToken);

                var phaseTopic = await db.PhaseTopics
                    .SingleOrDefaultAsync(
                        pt => pt.PhaseId == phase.Id && pt.TopicId == topicId,
                        cancellationToken);

                if (phaseTopic is null)
                {
                    db.PhaseTopics.Add(new PhaseTopic(phase.Id, topicId, phaseTopicSeed.Order));
                }
                else
                {
                    phaseTopic.SyncOrder(phaseTopicSeed.Order);
                }

                phaseTopicCount++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return new CurriculumImportResult(moduleCount, topicCount, phaseCount, phaseTopicCount);
    }

    private async Task SyncTaskTypesAsync(
        int topicId,
        IReadOnlyCollection<TaskType> desiredTaskTypes,
        CancellationToken cancellationToken)
    {
        var existing = await db.TopicTaskTypes
            .Where(tt => tt.TopicId == topicId)
            .ToListAsync(cancellationToken);

        var desired = desiredTaskTypes.OrEmpty().ToHashSet();
        db.TopicTaskTypes.RemoveRange(existing.Where(tt => !desired.Contains(tt.TaskType)));

        var existingTypes = existing.Select(tt => tt.TaskType).ToHashSet();
        foreach (var taskType in desired.Where(taskType => !existingTypes.Contains(taskType)))
        {
            db.TopicTaskTypes.Add(new TopicTaskType(topicId, taskType));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string SlugOrFallback(string? slug, string title)
    {
        return string.IsNullOrWhiteSpace(slug)
            ? Slugify(Required(title, "Title"))
            : slug.Trim();
    }

    private static string Slugify(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();

        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string Required(string? value, string fieldName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{fieldName} is required in the curriculum seed.")
            : value.Trim();
    }
}

internal static class CollectionExtensions
{
    public static IReadOnlyCollection<T> OrEmpty<T>(this IReadOnlyCollection<T>? source)
    {
        return source ?? [];
    }
}
