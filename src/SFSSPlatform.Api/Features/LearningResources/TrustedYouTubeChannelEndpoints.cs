using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;
using SFSSPlatform.Infrastructure.StudyContent;

namespace SFSSPlatform.Api.Features.LearningResources;

public static class TrustedYouTubeChannelEndpoints
{
    public static RouteGroupBuilder MapTrustedYouTubeChannelEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/learning-resources/channels");

        group.MapPost("/import-local", ImportLocalAsync)
            .WithName("ImportTrustedYouTubeChannels");

        group.MapGet("/", SearchAsync)
            .WithName("GetTrustedYouTubeChannels");

        group.MapGet("/source-file", GetSourceFile)
            .WithName("GetTrustedYouTubeChannelSourceFile");

        return group;
    }

    private static async Task<Ok<TrustedYouTubeChannelImportResult>> ImportLocalAsync(
        TrustedYouTubeChannelImporter importer,
        CancellationToken cancellationToken)
    {
        var result = await importer.ImportLocalAsync(cancellationToken);
        return TypedResults.Ok(result);
    }

    private static Ok<TrustedYouTubeChannelSourceFileResponse> GetSourceFile(TrustedYouTubeChannelImporter importer)
    {
        return TypedResults.Ok(new TrustedYouTubeChannelSourceFileResponse(importer.ResolveSourceFile()));
    }

    private static async Task<Ok<IReadOnlyCollection<TrustedYouTubeChannelResponse>>> SearchAsync(
        StudyPlatformDbContext db,
        CancellationToken cancellationToken,
        string? search = null,
        string? tag = null,
        TrustedChannelPriority? priority = null)
    {
        var query = db.TrustedYouTubeChannels.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(channel =>
                channel.Name.Contains(value)
                || channel.Url.Contains(value)
                || channel.Tags.Contains(value)
                || channel.Notes.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var value = tag.Trim();
            query = query.Where(channel => channel.Tags.Contains(value));
        }

        if (priority is not null)
        {
            query = query.Where(channel => channel.Priority == priority);
        }

        var channels = await query
            .OrderByDescending(channel => channel.Priority == TrustedChannelPriority.High)
            .ThenByDescending(channel => channel.Priority == TrustedChannelPriority.Medium)
            .ThenBy(channel => channel.Name)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<TrustedYouTubeChannelResponse>>(channels.Select(ToResponse).ToList());
    }

    private static TrustedYouTubeChannelResponse ToResponse(TrustedYouTubeChannel channel)
    {
        return new TrustedYouTubeChannelResponse(
            channel.Id,
            channel.Name,
            channel.Url,
            SplitTags(channel.Tags),
            channel.Priority,
            channel.Notes,
            channel.CreatedAt,
            channel.UpdatedAt);
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

public sealed record TrustedYouTubeChannelResponse(
    int Id,
    string Name,
    string Url,
    IReadOnlyCollection<string> Tags,
    TrustedChannelPriority Priority,
    string Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TrustedYouTubeChannelSourceFileResponse(string SourceFile);
