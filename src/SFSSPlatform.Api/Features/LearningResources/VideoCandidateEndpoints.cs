using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;
using SFSSPlatform.Infrastructure.StudyContent;

namespace SFSSPlatform.Api.Features.LearningResources;

public static class VideoCandidateEndpoints
{
    public static RouteGroupBuilder MapVideoCandidateEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/learning-resources/candidates");

        group.MapPost("/import-local", ImportLocalAsync)
            .WithName("ImportVideoCandidates");

        group.MapGet("/", SearchAsync)
            .WithName("GetVideoCandidates");

        group.MapGet("/source-file", GetSourceFile)
            .WithName("GetVideoCandidateSourceFile");

        group.MapPost("/{id:int}/accept", AcceptAsync)
            .WithName("AcceptVideoCandidate");

        group.MapPost("/{id:int}/reject", RejectAsync)
            .WithName("RejectVideoCandidate");

        return group;
    }

    private static async Task<Ok<VideoCandidateImportResult>> ImportLocalAsync(
        VideoCandidateImporter importer,
        CancellationToken cancellationToken)
    {
        var result = await importer.ImportLocalAsync(cancellationToken);
        return TypedResults.Ok(result);
    }

    private static Ok<VideoCandidateSourceFileResponse> GetSourceFile(VideoCandidateImporter importer)
    {
        return TypedResults.Ok(new VideoCandidateSourceFileResponse(importer.ResolveSourceFile()));
    }

    private static async Task<Ok<IReadOnlyCollection<VideoCandidateResponse>>> SearchAsync(
        StudyPlatformDbContext db,
        CancellationToken cancellationToken,
        string? search = null,
        string? tag = null,
        string? status = null,
        string? difficulty = null)
    {
        var query = db.VideoCandidates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(candidate =>
                candidate.Title.Contains(value)
                || candidate.ChannelName.Contains(value)
                || candidate.Summary.Contains(value)
                || candidate.Tags.Contains(value)
                || candidate.Notes.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var value = tag.Trim();
            query = query.Where(candidate => candidate.Tags.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<VideoCandidateStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(candidate => candidate.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(difficulty)
            && Enum.TryParse<VideoCandidateDifficulty>(difficulty, ignoreCase: true, out var parsedDifficulty))
        {
            query = query.Where(candidate => candidate.Difficulty == parsedDifficulty);
        }

        var candidates = await query
            .OrderBy(candidate => candidate.Status != VideoCandidateStatus.Candidate)
            .ThenByDescending(candidate => candidate.Difficulty == VideoCandidateDifficulty.Expert)
            .ThenByDescending(candidate => candidate.Difficulty == VideoCandidateDifficulty.Advanced)
            .ThenBy(candidate => candidate.ChannelName)
            .ThenBy(candidate => candidate.Title)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<VideoCandidateResponse>>(candidates.Select(ToResponse).ToList());
    }

    private static async Task<Results<Ok<AcceptVideoCandidateResponse>, NotFound>> AcceptAsync(
        int id,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var candidate = await db.VideoCandidates.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (candidate is null)
        {
            return TypedResults.NotFound();
        }

        var now = timeProvider.GetUtcNow();
        var resource = await db.LearningResources.SingleOrDefaultAsync(
            item => item.Provider == LearningResourceProvider.YouTube && item.ExternalId == candidate.ExternalId,
            cancellationToken);

        if (resource is null)
        {
            resource = new LearningResource(
                candidate.ExternalId,
                LearningResourceProvider.YouTube,
                candidate.Title,
                candidate.ChannelName,
                candidate.Url,
                candidate.EmbedUrl,
                candidate.Summary,
                candidate.Tags,
                null,
                now);
            db.LearningResources.Add(resource);
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            resource.SyncMetadata(
                candidate.Title,
                candidate.ChannelName,
                candidate.Url,
                candidate.EmbedUrl,
                candidate.Summary,
                candidate.Tags,
                resource.DurationSeconds,
                now);
        }

        candidate.Accept(resource.Id, now);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new AcceptVideoCandidateResponse(ToResponse(candidate), ToLearningResourceResponse(resource)));
    }

    private static async Task<Results<Ok<VideoCandidateResponse>, NotFound>> RejectAsync(
        int id,
        RejectVideoCandidateRequest request,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var candidate = await db.VideoCandidates.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (candidate is null)
        {
            return TypedResults.NotFound();
        }

        candidate.Reject(request.Reason, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(ToResponse(candidate));
    }

    private static VideoCandidateResponse ToResponse(VideoCandidate candidate)
    {
        return new VideoCandidateResponse(
            candidate.Id,
            candidate.ExternalId,
            candidate.Title,
            candidate.ChannelName,
            candidate.ChannelUrl,
            candidate.Url,
            candidate.EmbedUrl,
            candidate.Summary,
            SplitTags(candidate.Tags),
            candidate.Difficulty,
            candidate.Status,
            candidate.Notes,
            candidate.RejectionReason,
            candidate.LearningResourceId,
            candidate.AcceptedAt,
            candidate.RejectedAt,
            candidate.CreatedAt,
            candidate.UpdatedAt);
    }

    private static LearningResourceResponse ToLearningResourceResponse(LearningResource resource)
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

public sealed record VideoCandidateResponse(
    int Id,
    string ExternalId,
    string Title,
    string ChannelName,
    string ChannelUrl,
    string Url,
    string EmbedUrl,
    string Summary,
    IReadOnlyCollection<string> Tags,
    VideoCandidateDifficulty Difficulty,
    VideoCandidateStatus Status,
    string Notes,
    string RejectionReason,
    int? LearningResourceId,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? RejectedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record VideoCandidateSourceFileResponse(string SourceFile);

public sealed record AcceptVideoCandidateResponse(VideoCandidateResponse Candidate, LearningResourceResponse Resource);

public sealed record RejectVideoCandidateRequest(string Reason);
