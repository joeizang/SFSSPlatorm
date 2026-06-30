using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;
using SFSSPlatform.Infrastructure.StudyContent;

namespace SFSSPlatform.Api.Features.Sources;

public static class SourceEndpoints
{
    public static RouteGroupBuilder MapSourceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sources");

        group.MapPost("/ingest-local", IngestLocalSourcesAsync)
            .WithName("IngestLocalSources");

        group.MapGet("/", GetSourcesAsync)
            .WithName("GetSources");

        group.MapGet("/{id:int}", GetSourceAsync)
            .WithName("GetSource");

        group.MapGet("/{id:int}/chunks", GetSourceChunksAsync)
            .WithName("GetSourceChunks");

        group.MapGet("/chunks/{chunkId:int}", GetChunkAsync)
            .WithName("GetSourceChunk");

        return group;
    }

    private static async Task<Ok<LocalPdfIngestionResult>> IngestLocalSourcesAsync(
        LocalPdfIngestionService ingestionService,
        CancellationToken cancellationToken,
        bool force = false)
    {
        var result = await ingestionService.IngestFocusedSourcesAsync(force, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<SourceMaterialResponse>>> GetSourcesAsync(
        StudyPlatformDbContext db,
        CancellationToken cancellationToken)
    {
        var sources = await db.SourceMaterials
            .OrderBy(source => source.Title)
            .Select(source => new SourceMaterialResponse(
                source.Id,
                source.Title,
                source.Author,
                source.FileName,
                source.Access,
                source.FileSizeBytes,
                source.PageCount,
                source.ExtractionStatus,
                source.ExtractionError,
                source.ExtractedAt,
                source.Chunks.Count,
                source.Chunks.Sum(chunk => chunk.CharacterCount)))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<SourceMaterialResponse>>(sources);
    }

    private static async Task<Results<Ok<SourceMaterialResponse>, NotFound>> GetSourceAsync(
        StudyPlatformDbContext db,
        int id,
        CancellationToken cancellationToken)
    {
        var source = await db.SourceMaterials
            .Where(source => source.Id == id)
            .Select(source => new SourceMaterialResponse(
                source.Id,
                source.Title,
                source.Author,
                source.FileName,
                source.Access,
                source.FileSizeBytes,
                source.PageCount,
                source.ExtractionStatus,
                source.ExtractionError,
                source.ExtractedAt,
                source.Chunks.Count,
                source.Chunks.Sum(chunk => chunk.CharacterCount)))
            .SingleOrDefaultAsync(cancellationToken);

        return source is null ? TypedResults.NotFound() : TypedResults.Ok(source);
    }

    private static async Task<Ok<IReadOnlyCollection<SourceChunkSummaryResponse>>> GetSourceChunksAsync(
        StudyPlatformDbContext db,
        int id,
        CancellationToken cancellationToken)
    {
        var chunks = await db.SourceDocumentChunks
            .Where(chunk => chunk.SourceMaterialId == id)
            .OrderBy(chunk => chunk.Order)
            .Select(chunk => new SourceChunkSummaryResponse(
                chunk.Id,
                chunk.Order,
                chunk.StartPage,
                chunk.EndPage,
                chunk.Heading,
                chunk.CharacterCount))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<SourceChunkSummaryResponse>>(chunks);
    }

    private static async Task<Results<Ok<SourceChunkResponse>, NotFound>> GetChunkAsync(
        StudyPlatformDbContext db,
        int chunkId,
        CancellationToken cancellationToken)
    {
        var chunk = await db.SourceDocumentChunks
            .Where(chunk => chunk.Id == chunkId)
            .Select(chunk => new SourceChunkResponse(
                chunk.Id,
                chunk.SourceMaterialId,
                chunk.SourceMaterial.Title,
                chunk.Order,
                chunk.StartPage,
                chunk.EndPage,
                chunk.Heading,
                chunk.Text,
                chunk.CharacterCount))
            .SingleOrDefaultAsync(cancellationToken);

        return chunk is null ? TypedResults.NotFound() : TypedResults.Ok(chunk);
    }
}

public sealed record SourceMaterialResponse(
    int Id,
    string Title,
    string? Author,
    string FileName,
    SourceAccess Access,
    long FileSizeBytes,
    int? PageCount,
    ExtractionStatus ExtractionStatus,
    string? ExtractionError,
    DateTimeOffset? ExtractedAt,
    int ChunkCount,
    int CharacterCount);

public sealed record SourceChunkSummaryResponse(
    int Id,
    int Order,
    int StartPage,
    int EndPage,
    string Heading,
    int CharacterCount);

public sealed record SourceChunkResponse(
    int Id,
    int SourceMaterialId,
    string SourceTitle,
    int Order,
    int StartPage,
    int EndPage,
    string Heading,
    string Text,
    int CharacterCount);
