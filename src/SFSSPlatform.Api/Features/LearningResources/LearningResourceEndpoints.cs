using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;
using SFSSPlatform.Infrastructure.StudyContent;

namespace SFSSPlatform.Api.Features.LearningResources;

public static class LearningResourceEndpoints
{
    public static RouteGroupBuilder MapLearningResourceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/learning-resources");

        group.MapPost("/import-seed", ImportSeedAsync)
            .WithName("ImportLearningResources");

        group.MapGet("/", SearchAsync)
            .WithName("GetLearningResources");

        group.MapGet("/{id:int}", GetAsync)
            .WithName("GetLearningResource");

        group.MapPut("/{id:int}/watch-state", UpdateWatchStateAsync)
            .WithName("UpdateLearningResourceWatchState");

        group.MapPut("/{id:int}/source-link", AttachSourceAsync)
            .WithName("AttachLearningResourceSource");

        return group;
    }

    private static async Task<Ok<LearningResourceImportResult>> ImportSeedAsync(
        LearningResourceSeeder seeder,
        CancellationToken cancellationToken)
    {
        var result = await seeder.ImportCuratedVideosAsync(cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<LearningResourceResponse>>> SearchAsync(
        StudyPlatformDbContext db,
        CancellationToken cancellationToken,
        string? search = null,
        string? tag = null,
        bool? watched = null)
    {
        var query = db.LearningResources.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(resource =>
                resource.Title.Contains(value)
                || resource.Creator.Contains(value)
                || resource.Summary.Contains(value)
                || resource.Tags.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var value = tag.Trim();
            query = query.Where(resource => resource.Tags.Contains(value));
        }

        if (watched is not null)
        {
            query = query.Where(resource => resource.IsWatched == watched);
        }

        var resources = await query
            .OrderBy(resource => resource.IsWatched)
            .ThenBy(resource => resource.Title)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<LearningResourceResponse>>(resources.Select(ToResponse).ToList());
    }

    private static async Task<Results<Ok<LearningResourceResponse>, NotFound>> GetAsync(
        int id,
        StudyPlatformDbContext db,
        CancellationToken cancellationToken)
    {
        var resource = await db.LearningResources
            .AsNoTracking()
            .Where(resource => resource.Id == id)
            .SingleOrDefaultAsync(cancellationToken);

        return resource is null ? TypedResults.NotFound() : TypedResults.Ok(ToResponse(resource));
    }

    private static async Task<Results<Ok<LearningResourceResponse>, NotFound>> UpdateWatchStateAsync(
        int id,
        UpdateLearningResourceWatchStateRequest request,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var resource = await db.LearningResources.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (resource is null)
        {
            return TypedResults.NotFound();
        }

        resource.UpdateWatchState(
            request.IsWatched,
            request.WatchProgressSeconds,
            request.Notes,
            timeProvider.GetUtcNow());

        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(ToResponse(resource));
    }

    private static async Task<Results<Ok<LearningResourceResponse>, NotFound>> AttachSourceAsync(
        int id,
        AttachLearningResourceRequest request,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var resource = await db.LearningResources.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (resource is null)
        {
            return TypedResults.NotFound();
        }

        if (request.SourceMaterialId is not null)
        {
            var sourceExists = await db.SourceMaterials.AnyAsync(source => source.Id == request.SourceMaterialId, cancellationToken);
            if (!sourceExists)
            {
                return TypedResults.NotFound();
            }
        }

        if (request.SourceDocumentChunkId is not null)
        {
            var chunkExists = await db.SourceDocumentChunks.AnyAsync(chunk => chunk.Id == request.SourceDocumentChunkId, cancellationToken);
            if (!chunkExists)
            {
                return TypedResults.NotFound();
            }
        }

        resource.AttachToSource(request.SourceMaterialId, request.SourceDocumentChunkId, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(ToResponse(resource));
    }

    private static LearningResourceResponse ToResponse(LearningResource resource)
    {
        return new LearningResourceResponse(
            resource.Id,
            resource.ExternalId,
            resource.Provider,
            resource.Title,
            resource.Creator,
            resource.Url,
            resource.EmbedUrl,
            resource.Summary,
            SplitTags(resource.Tags),
            resource.DurationSeconds,
            resource.TopicId,
            resource.SourceMaterialId,
            resource.SourceDocumentChunkId,
            resource.IsWatched,
            resource.WatchedAt,
            resource.WatchProgressSeconds,
            resource.Notes,
            resource.CreatedAt,
            resource.UpdatedAt);
    }

    private static IReadOnlyCollection<string> SplitTags(string tags)
    {
        return tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

public sealed record LearningResourceResponse(
    int Id,
    string ExternalId,
    LearningResourceProvider Provider,
    string Title,
    string Creator,
    string Url,
    string EmbedUrl,
    string Summary,
    IReadOnlyCollection<string> Tags,
    int? DurationSeconds,
    int? TopicId,
    int? SourceMaterialId,
    int? SourceDocumentChunkId,
    bool IsWatched,
    DateTimeOffset? WatchedAt,
    int WatchProgressSeconds,
    string Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpdateLearningResourceWatchStateRequest(bool IsWatched, int WatchProgressSeconds, string Notes);

public sealed record AttachLearningResourceRequest(int? SourceMaterialId, int? SourceDocumentChunkId);
