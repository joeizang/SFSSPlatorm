using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;
using SFSSPlatform.Infrastructure.StudyContent;

namespace SFSSPlatform.Api.Features.StudyItems;

public static class StudyItemEndpoints
{
    public static RouteGroupBuilder MapStudyItemEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/study-items");

        group.MapPost("/generate", GenerateAsync)
            .WithName("GenerateStudyItems");

        group.MapPost("/", CreateAsync)
            .WithName("CreateStudyItems");

        group.MapGet("/", SearchAsync)
            .WithName("GetStudyItems");

        group.MapPut("/{id:int}", UpdateAsync)
            .WithName("UpdateStudyItem");

        return group;
    }

    private static async Task<Results<Ok<GeneratedStudyItemsResponse>, NotFound>> GenerateAsync(
        GenerateStudyItemsRequest request,
        StudyPlatformDbContext db,
        StudyItemGenerator generator,
        CancellationToken cancellationToken)
    {
        var chunk = await db.SourceDocumentChunks
            .AsNoTracking()
            .Where(sourceChunk => sourceChunk.Id == request.SourceDocumentChunkId)
            .Select(sourceChunk => new
            {
                sourceChunk.Id,
                sourceChunk.SourceMaterialId,
                sourceChunk.StartPage,
                sourceChunk.EndPage,
                sourceChunk.SourceMaterial.Title
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (chunk is null)
        {
            return TypedResults.NotFound();
        }

        var drafts = await generator.GenerateForChunkAsync(request.SourceDocumentChunkId, cancellationToken);
        var response = new GeneratedStudyItemsResponse(
            chunk.Id,
            chunk.SourceMaterialId,
            chunk.Title,
            chunk.StartPage,
            chunk.EndPage,
            drafts.Select(draft => new StudyItemDraftResponse(
                draft.Kind,
                draft.Prompt,
                draft.ExpectedAnswer,
                draft.Explanation,
                draft.SourceExcerpt)).ToList());

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<StudyItemResponse>>, NotFound>> CreateAsync(
        CreateStudyItemsRequest request,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var chunk = await db.SourceDocumentChunks
            .AsNoTracking()
            .Where(sourceChunk => sourceChunk.Id == request.SourceDocumentChunkId)
            .Select(sourceChunk => new
            {
                sourceChunk.Id,
                sourceChunk.SourceMaterialId,
                sourceChunk.StartPage,
                sourceChunk.EndPage
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (chunk is null)
        {
            return TypedResults.NotFound();
        }

        var now = timeProvider.GetUtcNow();
        var items = request.Items
            .Where(item => !string.IsNullOrWhiteSpace(item.Prompt) && !string.IsNullOrWhiteSpace(item.ExpectedAnswer))
            .Select(item => new StudyItem(
                chunk.SourceMaterialId,
                chunk.Id,
                item.Kind,
                item.Prompt,
                item.ExpectedAnswer,
                item.Explanation,
                item.SourceExcerpt,
                chunk.StartPage,
                chunk.EndPage,
                now))
            .ToList();

        db.StudyItems.AddRange(items);
        await db.SaveChangesAsync(cancellationToken);

        var responses = items.Select(ToResponse).ToList();
        return TypedResults.Ok<IReadOnlyCollection<StudyItemResponse>>(responses);
    }

    private static async Task<Ok<IReadOnlyCollection<StudyItemResponse>>> SearchAsync(
        StudyPlatformDbContext db,
        CancellationToken cancellationToken,
        int? sourceDocumentChunkId = null,
        string? status = null)
    {
        var query = db.StudyItems.AsNoTracking();

        if (sourceDocumentChunkId is not null)
        {
            query = query.Where(item => item.SourceDocumentChunkId == sourceDocumentChunkId);
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<StudyItemStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(item => item.Status == parsedStatus);
        }

        var items = await query
            .OrderByDescending(item => item.Id)
            .Select(item => new StudyItemResponse(
                item.Id,
                item.SourceMaterialId,
                item.SourceDocumentChunkId,
                item.Kind,
                item.Prompt,
                item.ExpectedAnswer,
                item.Explanation,
                item.SourceExcerpt,
                item.StartPage,
                item.EndPage,
                item.Status,
                item.AttemptCount,
                item.ConfidenceScore,
                item.LastAttemptAt,
                item.NextReviewAt,
                item.CreatedAt,
                item.UpdatedAt))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<StudyItemResponse>>(items);
    }

    private static async Task<Results<Ok<StudyItemResponse>, NotFound>> UpdateAsync(
        int id,
        UpdateStudyItemRequest request,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var item = await db.StudyItems.SingleOrDefaultAsync(studyItem => studyItem.Id == id, cancellationToken);
        if (item is null)
        {
            return TypedResults.NotFound();
        }

        item.Update(
            request.Kind,
            request.Prompt,
            request.ExpectedAnswer,
            request.Explanation,
            request.Status,
            timeProvider.GetUtcNow());

        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(ToResponse(item));
    }

    private static StudyItemResponse ToResponse(StudyItem item)
    {
        return new StudyItemResponse(
            item.Id,
            item.SourceMaterialId,
            item.SourceDocumentChunkId,
            item.Kind,
            item.Prompt,
            item.ExpectedAnswer,
            item.Explanation,
            item.SourceExcerpt,
            item.StartPage,
            item.EndPage,
            item.Status,
            item.AttemptCount,
            item.ConfidenceScore,
            item.LastAttemptAt,
            item.NextReviewAt,
            item.CreatedAt,
            item.UpdatedAt);
    }
}

public sealed record GenerateStudyItemsRequest(int SourceDocumentChunkId);

public sealed record GeneratedStudyItemsResponse(
    int SourceDocumentChunkId,
    int SourceMaterialId,
    string SourceTitle,
    int StartPage,
    int EndPage,
    IReadOnlyCollection<StudyItemDraftResponse> Items);

public sealed record StudyItemDraftResponse(
    StudyItemKind Kind,
    string Prompt,
    string ExpectedAnswer,
    string Explanation,
    string SourceExcerpt);

public sealed record CreateStudyItemsRequest(
    int SourceDocumentChunkId,
    IReadOnlyCollection<CreateStudyItemRequest> Items);

public sealed record CreateStudyItemRequest(
    StudyItemKind Kind,
    string Prompt,
    string ExpectedAnswer,
    string Explanation,
    string SourceExcerpt);

public sealed record UpdateStudyItemRequest(
    StudyItemKind Kind,
    string Prompt,
    string ExpectedAnswer,
    string Explanation,
    StudyItemStatus Status);

public sealed record StudyItemResponse(
    int Id,
    int SourceMaterialId,
    int SourceDocumentChunkId,
    StudyItemKind Kind,
    string Prompt,
    string ExpectedAnswer,
    string Explanation,
    string SourceExcerpt,
    int StartPage,
    int EndPage,
    StudyItemStatus Status,
    int AttemptCount,
    int ConfidenceScore,
    DateTimeOffset? LastAttemptAt,
    DateTimeOffset? NextReviewAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
