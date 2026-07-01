using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Api.Features.LearningResources;

public static class TopicResourceLinkEndpoints
{
    public static RouteGroupBuilder MapTopicResourceLinkEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/topic-resource-links");

        group.MapGet("/", SearchAsync)
            .WithName("GetTopicResourceLinks");

        group.MapPost("/", UpsertAsync)
            .WithName("UpsertTopicResourceLink");

        group.MapDelete("/{id:int}", DeleteAsync)
            .WithName("DeleteTopicResourceLink");

        group.MapGet("/unlinked", GetUnlinkedAsync)
            .WithName("GetUnlinkedTopicResources");

        return group;
    }

    private static async Task<Ok<IReadOnlyCollection<TopicResourceLinkResponse>>> SearchAsync(
        StudyPlatformDbContext db,
        CancellationToken cancellationToken,
        string? topicSlug = null)
    {
        var query = db.TopicResourceLinks
            .AsNoTracking()
            .Include(link => link.Topic)
                .ThenInclude(topic => topic.Module)
            .Include(link => link.LearningResource)
            .Include(link => link.VideoCandidate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(topicSlug))
        {
            var slug = topicSlug.Trim();
            query = query.Where(link => link.Topic.Slug == slug);
        }

        var links = await query
            .OrderBy(link => link.Topic.Module.Order)
            .ThenBy(link => link.Topic.Order)
            .ThenByDescending(link => link.Priority)
            .ThenBy(link => link.LearningResource != null ? link.LearningResource.Title : link.VideoCandidate!.Title)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<TopicResourceLinkResponse>>(links.Select(ToResponse).ToList());
    }

    private static async Task<Results<Ok<TopicResourceLinkResponse>, NotFound, BadRequest<string>>> UpsertAsync(
        UpsertTopicResourceLinkRequest request,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if ((request.LearningResourceId is null) == (request.VideoCandidateId is null))
        {
            return TypedResults.BadRequest("A topic resource link requires exactly one learning resource or video candidate.");
        }

        var topic = await db.Topics
            .SingleOrDefaultAsync(item => item.Slug == request.TopicSlug, cancellationToken);
        if (topic is null)
        {
            return TypedResults.NotFound();
        }

        if (request.LearningResourceId is not null)
        {
            var resourceExists = await db.LearningResources
                .AnyAsync(resource => resource.Id == request.LearningResourceId, cancellationToken);
            if (!resourceExists)
            {
                return TypedResults.NotFound();
            }
        }

        if (request.VideoCandidateId is not null)
        {
            var candidateExists = await db.VideoCandidates
                .AnyAsync(candidate => candidate.Id == request.VideoCandidateId, cancellationToken);
            if (!candidateExists)
            {
                return TypedResults.NotFound();
            }
        }

        var existing = await db.TopicResourceLinks
            .SingleOrDefaultAsync(
                link => link.TopicId == topic.Id
                    && link.LearningResourceId == request.LearningResourceId
                    && link.VideoCandidateId == request.VideoCandidateId,
                cancellationToken);

        var now = timeProvider.GetUtcNow();
        if (existing is null)
        {
            existing = new TopicResourceLink(
                topic.Id,
                request.LearningResourceId,
                request.VideoCandidateId,
                request.Priority,
                request.Notes,
                now);
            db.TopicResourceLinks.Add(existing);
        }
        else
        {
            existing.Update(request.Priority, request.Notes, now);
        }

        await db.SaveChangesAsync(cancellationToken);

        var saved = await db.TopicResourceLinks
            .AsNoTracking()
            .Include(link => link.Topic)
                .ThenInclude(savedTopic => savedTopic.Module)
            .Include(link => link.LearningResource)
            .Include(link => link.VideoCandidate)
            .SingleAsync(link => link.Id == existing.Id, cancellationToken);

        return TypedResults.Ok(ToResponse(saved));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id,
        StudyPlatformDbContext db,
        CancellationToken cancellationToken)
    {
        var deleted = await db.TopicResourceLinks
            .Where(link => link.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted == 0 ? TypedResults.NotFound() : TypedResults.NoContent();
    }

    private static async Task<Ok<UnlinkedTopicResourcesResponse>> GetUnlinkedAsync(
        StudyPlatformDbContext db,
        CancellationToken cancellationToken)
    {
        var linkedResourceIds = db.TopicResourceLinks
            .Where(link => link.LearningResourceId != null)
            .Select(link => link.LearningResourceId!.Value);
        var linkedCandidateIds = db.TopicResourceLinks
            .Where(link => link.VideoCandidateId != null)
            .Select(link => link.VideoCandidateId!.Value);

        var resourceRows = await db.LearningResources
            .AsNoTracking()
            .Where(resource => !linkedResourceIds.Contains(resource.Id))
            .OrderBy(resource => resource.Title)
            .ToListAsync(cancellationToken);

        var candidateRows = await db.VideoCandidates
            .AsNoTracking()
            .Where(candidate => !linkedCandidateIds.Contains(candidate.Id))
            .OrderBy(candidate => candidate.Status != VideoCandidateStatus.Candidate)
            .ThenBy(candidate => candidate.Title)
            .ToListAsync(cancellationToken);

        var resources = resourceRows
            .Select(resource => new UnlinkedLearningResourceResponse(
                resource.Id,
                resource.Title,
                resource.Creator,
                SplitTags(resource.Tags)))
            .ToList();

        var candidates = candidateRows
            .Select(candidate => new UnlinkedVideoCandidateResponse(
                candidate.Id,
                candidate.Title,
                candidate.ChannelName,
                candidate.Difficulty,
                candidate.Status,
                SplitTags(candidate.Tags)))
            .ToList();

        return TypedResults.Ok(new UnlinkedTopicResourcesResponse(resources, candidates));
    }

    private static TopicResourceLinkResponse ToResponse(TopicResourceLink link)
    {
        var title = link.LearningResource?.Title ?? link.VideoCandidate?.Title ?? "Resource";
        var provider = link.LearningResource is not null ? "learningResource" : "videoCandidate";
        var creator = link.LearningResource?.Creator ?? link.VideoCandidate?.ChannelName ?? string.Empty;
        var url = link.LearningResource?.Url ?? link.VideoCandidate?.Url ?? string.Empty;
        var embedUrl = link.LearningResource?.EmbedUrl ?? link.VideoCandidate?.EmbedUrl ?? string.Empty;
        var tags = link.LearningResource is not null
            ? SplitTags(link.LearningResource.Tags)
            : SplitTags(link.VideoCandidate?.Tags ?? string.Empty);

        return new TopicResourceLinkResponse(
            link.Id,
            link.Topic.Slug,
            link.Topic.Title,
            link.Topic.Module.Slug,
            link.Topic.Module.Title,
            link.LearningResourceId,
            link.VideoCandidateId,
            provider,
            title,
            creator,
            url,
            embedUrl,
            tags,
            link.Priority,
            link.Notes,
            link.CreatedAt,
            link.UpdatedAt);
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

public sealed record UpsertTopicResourceLinkRequest(
    string TopicSlug,
    int? LearningResourceId,
    int? VideoCandidateId,
    int Priority,
    string Notes);

public sealed record TopicResourceLinkResponse(
    int Id,
    string TopicSlug,
    string TopicTitle,
    string ModuleSlug,
    string ModuleTitle,
    int? LearningResourceId,
    int? VideoCandidateId,
    string ResourceKind,
    string Title,
    string Creator,
    string Url,
    string EmbedUrl,
    IReadOnlyCollection<string> Tags,
    int Priority,
    string Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UnlinkedTopicResourcesResponse(
    IReadOnlyCollection<UnlinkedLearningResourceResponse> LearningResources,
    IReadOnlyCollection<UnlinkedVideoCandidateResponse> VideoCandidates);

public sealed record UnlinkedLearningResourceResponse(
    int Id,
    string Title,
    string Creator,
    IReadOnlyCollection<string> Tags);

public sealed record UnlinkedVideoCandidateResponse(
    int Id,
    string Title,
    string ChannelName,
    VideoCandidateDifficulty Difficulty,
    VideoCandidateStatus Status,
    IReadOnlyCollection<string> Tags);
