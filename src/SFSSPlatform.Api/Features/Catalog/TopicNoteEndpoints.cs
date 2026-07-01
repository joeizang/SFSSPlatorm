using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.Curriculum;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Api.Features.Catalog;

public static class TopicNoteEndpoints
{
    public static RouteGroupBuilder MapTopicNoteEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/catalog/topics/{slug}/notes");

        group.MapGet("/", GetAsync)
            .WithName("GetTopicNote");

        group.MapPut("/", UpsertAsync)
            .WithName("UpsertTopicNote");

        return group;
    }

    private static async Task<Results<Ok<TopicNoteResponse>, NotFound>> GetAsync(
        string slug,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var topic = await db.Topics
            .AsNoTracking()
            .Where(topic => topic.Slug == slug)
            .Select(topic => new { topic.Id, topic.Slug })
            .SingleOrDefaultAsync(cancellationToken);
        if (topic is null)
        {
            return TypedResults.NotFound();
        }

        var note = await db.TopicNotes
            .AsNoTracking()
            .Where(note => note.TopicId == topic.Id)
            .Select(note => new TopicNoteResponse(
                topic.Slug,
                note.Content,
                note.CreatedAt,
                note.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return TypedResults.Ok(note ?? new TopicNoteResponse(
            topic.Slug,
            string.Empty,
            timeProvider.GetUtcNow(),
            timeProvider.GetUtcNow()));
    }

    private static async Task<Results<Ok<TopicNoteResponse>, NotFound, BadRequest<string>>> UpsertAsync(
        string slug,
        UpsertTopicNoteRequest request,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (request.Content.Length > 40_000)
        {
            return TypedResults.BadRequest("Topic notes cannot exceed 40000 characters.");
        }

        var topic = await db.Topics
            .SingleOrDefaultAsync(topic => topic.Slug == slug, cancellationToken);
        if (topic is null)
        {
            return TypedResults.NotFound();
        }

        var note = await db.TopicNotes
            .SingleOrDefaultAsync(note => note.TopicId == topic.Id, cancellationToken);

        var now = timeProvider.GetUtcNow();
        if (note is null)
        {
            note = new TopicNote(topic.Id, request.Content, now);
            db.TopicNotes.Add(note);
        }
        else
        {
            note.Update(request.Content, now);
        }

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new TopicNoteResponse(
            topic.Slug,
            note.Content,
            note.CreatedAt,
            note.UpdatedAt));
    }
}

public sealed record TopicNoteResponse(
    string TopicSlug,
    string Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertTopicNoteRequest(string Content);
