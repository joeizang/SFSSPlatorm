using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Api.Features.StudySession;

public static class StudySessionEndpoints
{
    public static RouteGroupBuilder MapStudySessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/study-session");

        group.MapGet("/next", GetNextAsync)
            .WithName("GetNextStudySessionItem");

        group.MapPost("/attempts", RecordAttemptAsync)
            .WithName("RecordStudyAttempt");

        group.MapGet("/summary", GetSummaryAsync)
            .WithName("GetStudySessionSummary");

        return group;
    }

    private static async Task<Ok<StudySessionNextResponse>> GetNextAsync(
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var activeItems = await db.StudyItems
            .AsNoTracking()
            .Where(item => item.Status == StudyItemStatus.Active)
            .Select(item => new StudySessionItemResponse(
                item.Id,
                item.Kind,
                item.Prompt,
                item.ExpectedAnswer,
                item.Explanation,
                item.SourceExcerpt,
                item.StartPage,
                item.EndPage,
                item.SourceMaterial.Title,
                item.SourceDocumentChunk.Heading,
                item.AttemptCount,
                item.ConfidenceScore,
                item.LastAttemptAt,
                item.NextReviewAt))
            .ToListAsync(cancellationToken);

        var dueItems = activeItems
            .Where(item => item.NextReviewAt is null || item.NextReviewAt <= now)
            .OrderBy(item => item.NextReviewAt is null ? 0 : 1)
            .ThenBy(item => item.ConfidenceScore)
            .ThenBy(item => item.Id)
            .ToList();

        var item = dueItems.FirstOrDefault();
        var dueCount = dueItems.Count;
        return TypedResults.Ok(new StudySessionNextResponse(item, dueCount));
    }

    private static async Task<Results<Ok<StudyAttemptResponse>, NotFound>> RecordAttemptAsync(
        RecordStudyAttemptRequest request,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var item = await db.StudyItems
            .SingleOrDefaultAsync(studyItem => studyItem.Id == request.StudyItemId, cancellationToken);

        if (item is null)
        {
            return TypedResults.NotFound();
        }

        var now = timeProvider.GetUtcNow();
        var attempt = new StudyAttempt(item.Id, request.Answer, request.Rating, now);
        item.RecordAttempt(request.Rating, now);
        db.StudyAttempts.Add(attempt);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new StudyAttemptResponse(
            attempt.Id,
            item.Id,
            attempt.Answer,
            attempt.Rating,
            attempt.AttemptedAt,
            item.AttemptCount,
            item.ConfidenceScore,
            item.NextReviewAt));
    }

    private static async Task<Ok<StudySessionSummaryResponse>> GetSummaryAsync(
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var startOfDay = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);

        var activeCount = await db.StudyItems.CountAsync(item => item.Status == StudyItemStatus.Active, cancellationToken);
        var reviewDates = await db.StudyItems
            .AsNoTracking()
            .Where(item => item.Status == StudyItemStatus.Active)
            .Select(item => item.NextReviewAt)
            .ToListAsync(cancellationToken);
        var dueCount = reviewDates.Count(nextReviewAt => nextReviewAt is null || nextReviewAt <= now);
        var attempts = await db.StudyAttempts
            .AsNoTracking()
            .Select(attempt => attempt.AttemptedAt)
            .ToListAsync(cancellationToken);
        var answeredToday = attempts.Count(attemptedAt => attemptedAt >= startOfDay);
        var weakCount = await db.StudyItems.CountAsync(
            item => item.Status == StudyItemStatus.Active && item.AttemptCount > 0 && item.ConfidenceScore <= 2,
            cancellationToken);

        return TypedResults.Ok(new StudySessionSummaryResponse(activeCount, dueCount, answeredToday, weakCount));
    }

}

public sealed record StudySessionNextResponse(
    StudySessionItemResponse? Item,
    int DueCount);

public sealed record StudySessionItemResponse(
    int Id,
    StudyItemKind Kind,
    string Prompt,
    string ExpectedAnswer,
    string Explanation,
    string SourceExcerpt,
    int StartPage,
    int EndPage,
    string SourceTitle,
    string SourceChunkHeading,
    int AttemptCount,
    int ConfidenceScore,
    DateTimeOffset? LastAttemptAt,
    DateTimeOffset? NextReviewAt);

public sealed record RecordStudyAttemptRequest(
    int StudyItemId,
    string Answer,
    StudyAttemptRating Rating);

public sealed record StudyAttemptResponse(
    int Id,
    int StudyItemId,
    string Answer,
    StudyAttemptRating Rating,
    DateTimeOffset AttemptedAt,
    int AttemptCount,
    int ConfidenceScore,
    DateTimeOffset? NextReviewAt);

public sealed record StudySessionSummaryResponse(
    int ActiveItems,
    int DueItems,
    int AnsweredToday,
    int WeakItems);
