using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Api.Features.StudyItems;

public static class CodingExerciseEndpoints
{
    public static RouteGroupBuilder MapCodingExerciseEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/catalog/topics/{slug}/exercises");

        group.MapGet("/", SearchAsync)
            .WithName("GetTopicCodingExercises");

        group.MapPost("/generate", GenerateAsync)
            .WithName("GenerateTopicCodingExercise");

        group.MapGet("/{exerciseId:int}/solution", GetSolutionAsync)
            .WithName("GetCodingExerciseSolution");

        group.MapPut("/{exerciseId:int}/solution", SaveSolutionAsync)
            .WithName("SaveCodingExerciseSolution");

        group.MapPost("/{exerciseId:int}/diagnostics", AnalyzeAsync)
            .WithName("AnalyzeCodingExerciseCode");

        group.MapPost("/{exerciseId:int}/check", CheckAsync)
            .WithName("CheckCodingExerciseCode");

        return group;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<CodingExerciseResponse>>, NotFound>> SearchAsync(
        string slug,
        StudyPlatformDbContext db,
        CancellationToken cancellationToken)
    {
        var topicExists = await db.Topics.AnyAsync(topic => topic.Slug == slug, cancellationToken);
        if (!topicExists)
        {
            return TypedResults.NotFound();
        }

        var exercises = await db.CodingExercises
            .AsNoTracking()
            .Where(exercise => exercise.Topic.Slug == slug)
            .OrderBy(exercise => exercise.Difficulty)
            .ThenBy(exercise => exercise.Title)
            .Select(exercise => new CodingExerciseResponse(
                exercise.Id,
                slug,
                exercise.Title,
                exercise.Prompt,
                exercise.Difficulty,
                exercise.Language,
                exercise.StarterCode,
                exercise.PackageRequirements,
                exercise.SuccessCriteria,
                exercise.Hints,
                exercise.CheckDefinitionJson,
                exercise.CreatedAt,
                exercise.UpdatedAt))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyCollection<CodingExerciseResponse>>(exercises);
    }

    private static async Task<Results<Ok<CodingExerciseResponse>, NotFound>> GenerateAsync(
        string slug,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var topic = await db.Topics
            .SingleOrDefaultAsync(topic => topic.Slug == slug, cancellationToken);
        if (topic is null)
        {
            return TypedResults.NotFound();
        }

        var now = timeProvider.GetUtcNow();
        var title = $"{topic.Title} implementation drill";
        var existing = await db.CodingExercises
            .SingleOrDefaultAsync(
                exercise => exercise.TopicId == topic.Id && exercise.Title == title,
                cancellationToken);

        var prompt = $"Build a focused .NET example for {topic.Title}. Keep it small enough for the in-app editor, but production-shaped enough to include clear inputs, error handling, and a verifiable output.";
        var starterCode = $$"""
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            public static class Exercise
            {
                public static Task<string> RunAsync(string input, CancellationToken cancellationToken = default)
                {
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        throw new ArgumentException("Input is required.", nameof(input));
                    }

                    // TODO: Implement the {{topic.Title}} scenario.
                    return Task.FromResult(input.Trim());
                }
            }
            """;
        var packageRequirements = "NuGet: Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio. NPM: none for this C# exercise.";
        var successCriteria = "The solution should compile, preserve cancellation-token flow, reject invalid input, and include at least one success-path and one failure-path test.";
        var hints = "Start with the public contract. Keep infrastructure at the boundary. Make behavior observable through return values or tests before adding framework plumbing.";
        var checkDefinitionJson = $$"""
            {
              "summary": "Implement the required Exercise.RunAsync contract for {{topic.Title}} without changing the public method shape.",
              "visibleChecks": [
                {
                  "id": "compile",
                  "title": "Compiles as a C# library",
                  "description": "The submitted code must compile with the configured platform references."
                },
                {
                  "id": "contract",
                  "title": "Preserves the public contract",
                  "description": "Keep public static Task<string> RunAsync(string input, CancellationToken cancellationToken = default)."
                },
                {
                  "id": "invalid-input",
                  "title": "Rejects invalid input",
                  "description": "Guard null, empty, or whitespace input and throw ArgumentException."
                },
                {
                  "id": "cancellation",
                  "title": "Preserves cancellation flow",
                  "description": "Accept and observe the CancellationToken inside RunAsync."
                },
                {
                  "id": "implementation",
                  "title": "Replaces the placeholder",
                  "description": "The solution should contain meaningful implementation code instead of returning the starter expression unchanged."
                }
              ],
              "sampleCases": [
                {
                  "name": "trims input",
                  "input": "  route groups  ",
                  "expected": "ROUTE GROUPS"
                },
                {
                  "name": "rejects whitespace",
                  "input": "   ",
                  "expectedException": "ArgumentException"
                }
              ]
            }
            """;

        if (existing is null)
        {
            existing = new CodingExercise(
                topic.Id,
                title,
                prompt,
                CodingExerciseDifficulty.Intermediate,
                "csharp",
                starterCode,
                packageRequirements,
                successCriteria,
                hints,
                checkDefinitionJson,
                now);
            db.CodingExercises.Add(existing);
        }
        else
        {
            existing.Update(
                title,
                prompt,
                CodingExerciseDifficulty.Intermediate,
                "csharp",
                starterCode,
                packageRequirements,
                successCriteria,
                hints,
                checkDefinitionJson,
                now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new CodingExerciseResponse(
            existing.Id,
            slug,
            existing.Title,
            existing.Prompt,
            existing.Difficulty,
            existing.Language,
            existing.StarterCode,
            existing.PackageRequirements,
            existing.SuccessCriteria,
            existing.Hints,
            existing.CheckDefinitionJson,
            existing.CreatedAt,
            existing.UpdatedAt));
    }

    private static async Task<Results<Ok<CodingExerciseSolutionResponse>, NotFound>> GetSolutionAsync(
        string slug,
        int exerciseId,
        StudyPlatformDbContext db,
        CancellationToken cancellationToken)
    {
        var exercise = await db.CodingExercises
            .AsNoTracking()
            .Where(exercise => exercise.Id == exerciseId && exercise.Topic.Slug == slug)
            .Select(exercise => new
            {
                exercise.Id,
                exercise.StarterCode,
                exercise.CreatedAt,
                exercise.UpdatedAt
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (exercise is null)
        {
            return TypedResults.NotFound();
        }

        var solution = await db.CodingExerciseSolutions
            .AsNoTracking()
            .Where(solution => solution.CodingExerciseId == exerciseId)
            .Select(solution => new CodingExerciseSolutionResponse(
                solution.CodingExerciseId,
                slug,
                solution.Code,
                solution.CreatedAt,
                solution.UpdatedAt,
                solution.LastCheckedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return TypedResults.Ok(solution ?? new CodingExerciseSolutionResponse(
            exercise.Id,
            slug,
            exercise.StarterCode,
            exercise.CreatedAt,
            exercise.UpdatedAt,
            null));
    }

    private static async Task<Results<Ok<CodingExerciseSolutionResponse>, NotFound, BadRequest<string>>> SaveSolutionAsync(
        string slug,
        int exerciseId,
        SaveCodingExerciseSolutionRequest request,
        StudyPlatformDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return TypedResults.BadRequest("Code is required.");
        }

        var exerciseExists = await db.CodingExercises
            .AnyAsync(exercise => exercise.Id == exerciseId && exercise.Topic.Slug == slug, cancellationToken);
        if (!exerciseExists)
        {
            return TypedResults.NotFound();
        }

        var now = timeProvider.GetUtcNow();
        var solution = await db.CodingExerciseSolutions
            .SingleOrDefaultAsync(solution => solution.CodingExerciseId == exerciseId, cancellationToken);

        if (solution is null)
        {
            solution = new CodingExerciseSolution(exerciseId, request.Code, now);
            db.CodingExerciseSolutions.Add(solution);
        }
        else
        {
            solution.UpdateCode(request.Code, now);
        }

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new CodingExerciseSolutionResponse(
            exerciseId,
            slug,
            solution.Code,
            solution.CreatedAt,
            solution.UpdatedAt,
            solution.LastCheckedAt));
    }

    private static Task<Results<Ok<ExerciseCheckResponse>, NotFound, BadRequest<string>>> AnalyzeAsync(
        string slug,
        int exerciseId,
        AnalyzeExerciseCodeRequest request,
        StudyPlatformDbContext db,
        CSharpExerciseAnalyzer analyzer,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        return CheckCoreAsync(slug, exerciseId, request, db, analyzer, timeProvider, persistCheckTimestamp: false, cancellationToken);
    }

    private static Task<Results<Ok<ExerciseCheckResponse>, NotFound, BadRequest<string>>> CheckAsync(
        string slug,
        int exerciseId,
        AnalyzeExerciseCodeRequest request,
        StudyPlatformDbContext db,
        CSharpExerciseAnalyzer analyzer,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        return CheckCoreAsync(slug, exerciseId, request, db, analyzer, timeProvider, persistCheckTimestamp: true, cancellationToken);
    }

    private static async Task<Results<Ok<ExerciseCheckResponse>, NotFound, BadRequest<string>>> CheckCoreAsync(
        string slug,
        int exerciseId,
        AnalyzeExerciseCodeRequest request,
        StudyPlatformDbContext db,
        CSharpExerciseAnalyzer analyzer,
        TimeProvider timeProvider,
        bool persistCheckTimestamp,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return TypedResults.BadRequest("Code is required.");
        }

        var exercise = await db.CodingExercises
            .Include(exercise => exercise.Topic)
            .SingleOrDefaultAsync(exercise => exercise.Id == exerciseId && exercise.Topic.Slug == slug, cancellationToken);
        if (exercise is null)
        {
            return TypedResults.NotFound();
        }

        var checkedAt = timeProvider.GetUtcNow();
        var result = analyzer.Analyze(exercise, request.Code, checkedAt);

        if (persistCheckTimestamp)
        {
            var solution = await db.CodingExerciseSolutions
                .SingleOrDefaultAsync(solution => solution.CodingExerciseId == exerciseId, cancellationToken);
            if (solution is not null && string.Equals(solution.Code, request.Code, StringComparison.Ordinal))
            {
                solution.MarkChecked(checkedAt);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        return TypedResults.Ok(result);
    }
}

public sealed record CodingExerciseResponse(
    int Id,
    string TopicSlug,
    string Title,
    string Prompt,
    CodingExerciseDifficulty Difficulty,
    string Language,
    string StarterCode,
    string PackageRequirements,
    string SuccessCriteria,
    string Hints,
    string CheckDefinitionJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
