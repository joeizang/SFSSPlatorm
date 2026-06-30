using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.Curriculum;
using SFSSPlatform.Infrastructure.Curriculum;
using SFSSPlatform.Infrastructure.Persistence;
using TaskType = SFSSPlatform.Domain.Curriculum.TaskType;

namespace SFSSPlatform.Api.Features.Catalog;

public static class CatalogEndpoints
{
    public static RouteGroupBuilder MapCatalogEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api");

        group.MapPost("/curriculum/import", ImportCurriculumAsync)
            .WithName("ImportCurriculum");

        group.MapGet("/catalog/phases", GetPhasesAsync)
            .WithName("GetCatalogPhases");

        group.MapGet("/catalog/modules", GetModulesAsync)
            .WithName("GetCatalogModules");

        group.MapGet("/catalog/topics", SearchTopicsAsync)
            .WithName("SearchCatalogTopics");

        group.MapGet("/catalog/topics/{slug}", GetTopicAsync)
            .WithName("GetCatalogTopic");

        group.MapPut("/catalog/topics/{slug}/progress", UpdateTopicProgressAsync)
            .WithName("UpdateTopicProgress");

        group.MapGet("/catalog/rollups", GetRollupsAsync)
            .WithName("GetCatalogRollups");

        return group;
    }

    private static async Task<Ok<CurriculumImportResult>> ImportCurriculumAsync(
        CurriculumSeed seed,
        CurriculumImporter importer,
        CancellationToken cancellationToken)
    {
        var result = await importer.ImportAsync(seed, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<PhaseResponse>>> GetPhasesAsync(
        StudyPlatformDbContext db,
        CancellationToken cancellationToken)
    {
        var phases = await db.Phases
            .OrderBy(phase => phase.Order)
            .Select(phase => new PhaseResponse(
                phase.Slug,
                phase.Title,
                phase.Description,
                phase.Topics
                    .OrderBy(phaseTopic => phaseTopic.Order)
                    .Select(phaseTopic => new PhaseTopicResponse(
                        phaseTopic.Topic.Slug,
                        phaseTopic.Topic.Title,
                        phaseTopic.Topic.Module.Slug,
                        phaseTopic.Topic.Module.Title,
                        phaseTopic.Topic.Progress.Status))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<PhaseResponse>>(phases);
    }

    private static async Task<Ok<IReadOnlyCollection<ModuleResponse>>> GetModulesAsync(
        StudyPlatformDbContext db,
        CancellationToken cancellationToken)
    {
        var modules = await db.Modules
            .OrderBy(module => module.Order)
            .Select(module => new ModuleResponse(
                module.Slug,
                module.Title,
                module.Description,
                module.Topics.Count,
                module.Topics.Count(topic => topic.Progress.Status == TopicProgressStatus.Done),
                module.Topics
                    .OrderBy(topic => topic.Order)
                    .Select(topic => new ModuleTopicResponse(
                        topic.Slug,
                        topic.Title,
                        topic.Progress.Status))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<ModuleResponse>>(modules);
    }

    private static async Task<Ok<IReadOnlyCollection<TopicSearchResponse>>> SearchTopicsAsync(
        StudyPlatformDbContext db,
        string? search,
        string? moduleSlug,
        TopicProgressStatus? status,
        TaskType? taskType,
        CancellationToken cancellationToken)
    {
        var query = db.Topics.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(topic =>
                EF.Functions.Like(topic.Title, $"%{normalizedSearch}%")
                || (topic.Summary != null && EF.Functions.Like(topic.Summary, $"%{normalizedSearch}%")));
        }

        if (!string.IsNullOrWhiteSpace(moduleSlug))
        {
            var normalizedModuleSlug = moduleSlug.Trim();
            query = query.Where(topic => topic.Module.Slug == normalizedModuleSlug);
        }

        if (status is not null)
        {
            query = query.Where(topic => topic.Progress.Status == status);
        }

        if (taskType is not null)
        {
            query = query.Where(topic => topic.TaskTypes.Any(tt => tt.TaskType == taskType));
        }

        var topics = await query
            .OrderBy(topic => topic.Module.Order)
            .ThenBy(topic => topic.Order)
            .Select(topic => new TopicSearchResponse(
                topic.Slug,
                topic.Title,
                topic.Summary,
                topic.Module.Slug,
                topic.Module.Title,
                topic.Progress.Status,
                topic.TaskTypes.Select(tt => tt.TaskType).OrderBy(type => type).ToList()))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<TopicSearchResponse>>(topics);
    }

    private static async Task<Results<Ok<TopicDetailResponse>, NotFound>> GetTopicAsync(
        StudyPlatformDbContext db,
        string slug,
        CancellationToken cancellationToken)
    {
        var topic = await db.Topics
            .Where(topic => topic.Slug == slug)
            .Select(topic => new TopicDetailResponse(
                topic.Slug,
                topic.Title,
                topic.Summary,
                topic.Module.Slug,
                topic.Module.Title,
                topic.Progress.Status,
                topic.Progress.StartedAt,
                topic.Progress.CompletedAt,
                topic.Progress.UpdatedAt,
                topic.TaskTypes.Select(tt => tt.TaskType).OrderBy(type => type).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return topic is null ? TypedResults.NotFound() : TypedResults.Ok(topic);
    }

    private static async Task<Results<Ok<TopicDetailResponse>, NotFound, BadRequest<string>>> UpdateTopicProgressAsync(
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        string slug,
        UpdateTopicProgressRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Status))
        {
            return TypedResults.BadRequest("Unknown topic progress status.");
        }

        var topic = await db.Topics
            .SingleOrDefaultAsync(topic => topic.Slug == slug, cancellationToken);

        if (topic is null)
        {
            return TypedResults.NotFound();
        }

        topic.SetProgress(request.Status, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);

        var response = await db.Topics
            .Where(savedTopic => savedTopic.Id == topic.Id)
            .Select(savedTopic => new TopicDetailResponse(
                savedTopic.Slug,
                savedTopic.Title,
                savedTopic.Summary,
                savedTopic.Module.Slug,
                savedTopic.Module.Title,
                savedTopic.Progress.Status,
                savedTopic.Progress.StartedAt,
                savedTopic.Progress.CompletedAt,
                savedTopic.Progress.UpdatedAt,
                savedTopic.TaskTypes.Select(tt => tt.TaskType).OrderBy(type => type).ToList()))
            .SingleAsync(cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<CatalogRollupsResponse>> GetRollupsAsync(
        StudyPlatformDbContext db,
        CancellationToken cancellationToken)
    {
        var modules = await db.Modules
            .OrderBy(module => module.Order)
            .Select(module => new ModuleRollupResponse(
                module.Slug,
                module.Title,
                module.Topics.Count,
                module.Topics.Count(topic => topic.Progress.Status == TopicProgressStatus.Done),
                module.Topics.Count(topic => topic.Progress.Status == TopicProgressStatus.InProgress)))
            .ToListAsync(cancellationToken);

        var phases = await db.Phases
            .OrderBy(phase => phase.Order)
            .Select(phase => new PhaseRollupResponse(
                phase.Slug,
                phase.Title,
                phase.Topics.Count,
                phase.Topics.Count(phaseTopic => phaseTopic.Topic.Progress.Status == TopicProgressStatus.Done),
                phase.Topics.Count(phaseTopic => phaseTopic.Topic.Progress.Status == TopicProgressStatus.InProgress)))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(new CatalogRollupsResponse(phases, modules));
    }
}
