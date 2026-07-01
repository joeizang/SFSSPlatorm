using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFSSPlatform.Api.Features.StudyItems;
using SFSSPlatform.Domain.Curriculum;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Tests.StudyItems;

public sealed class StudyItemApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"sfssplatform-study-item-tests-{Guid.NewGuid():N}.db");

    private readonly WebApplicationFactory<Program> _factory;

    public StudyItemApiTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Default"] = $"Data Source={_databasePath}"
                    });
                });
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<StudyPlatformDbContext>>();
                    services.AddDbContext<StudyPlatformDbContext>(options => options.UseSqlite($"Data Source={_databasePath}"));
                });
            });
    }

    [Fact]
    public async Task Topic_study_flow_generates_saves_lists_questions_and_creates_exercise_skeleton()
    {
        using var client = _factory.CreateClient();
        var topicSlug = await SeedTopicAsync();

        var generatedResponse = await client.PostAsJsonAsync(
            "/api/study-items/generate-for-topic",
            new GenerateTopicStudyItemsRequest(topicSlug),
            JsonOptions);
        generatedResponse.EnsureSuccessStatusCode();

        var generated = await generatedResponse.Content.ReadFromJsonAsync<GeneratedStudyItemsResponse>(JsonOptions);
        Assert.NotNull(generated);
        Assert.Equal(topicSlug, generated.TopicSlug);
        Assert.Contains(generated.Items, item => item.Kind == StudyItemKind.CodingExercise);

        var selected = generated.Items.Take(2).ToList();
        var saveResponse = await client.PostAsJsonAsync(
            "/api/study-items/",
            new CreateStudyItemsRequest(
                null,
                topicSlug,
                selected.Select(item => new CreateStudyItemRequest(
                    item.Kind,
                    item.Prompt,
                    item.ExpectedAnswer,
                    item.Explanation,
                    item.SourceExcerpt)).ToList()),
            JsonOptions);
        saveResponse.EnsureSuccessStatusCode();

        var saved = await saveResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<StudyItemResponse>>(JsonOptions);
        Assert.NotNull(saved);
        Assert.Equal(2, saved.Count);
        Assert.All(saved, item =>
        {
            Assert.Equal(topicSlug, item.TopicSlug);
            Assert.Null(item.SourceDocumentChunkId);
        });

        var listed = await client.GetFromJsonAsync<IReadOnlyCollection<StudyItemResponse>>(
            $"/api/study-items/?topicSlug={topicSlug}",
            JsonOptions);
        Assert.NotNull(listed);
        Assert.Equal(2, listed.Count);

        var exerciseResponse = await client.PostAsync($"/api/catalog/topics/{topicSlug}/exercises/generate", null);
        exerciseResponse.EnsureSuccessStatusCode();

        var exercise = await exerciseResponse.Content.ReadFromJsonAsync<CodingExerciseResponse>(JsonOptions);
        Assert.NotNull(exercise);
        Assert.Equal(topicSlug, exercise.TopicSlug);
        Assert.Equal("csharp", exercise.Language);
        Assert.Contains("NuGet", exercise.PackageRequirements);
        Assert.Contains("RunAsync", exercise.StarterCode);

        var exercises = await client.GetFromJsonAsync<IReadOnlyCollection<CodingExerciseResponse>>(
            $"/api/catalog/topics/{topicSlug}/exercises/",
            JsonOptions);
        Assert.NotNull(exercises);
        Assert.Single(exercises);

        var solutionCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            public static class Exercise
            {
                public static Task<string> RunAsync(string input, CancellationToken cancellationToken = default)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        throw new ArgumentException("Input is required.", nameof(input));
                    }

                    return Task.FromResult(input.Trim().ToUpperInvariant());
                }
            }
            """;

        var saveSolutionResponse = await client.PutAsJsonAsync(
            $"/api/catalog/topics/{topicSlug}/exercises/{exercise.Id}/solution",
            new SaveCodingExerciseSolutionRequest(solutionCode),
            JsonOptions);
        saveSolutionResponse.EnsureSuccessStatusCode();

        var savedSolution = await saveSolutionResponse.Content.ReadFromJsonAsync<CodingExerciseSolutionResponse>(JsonOptions);
        Assert.NotNull(savedSolution);
        Assert.Equal(exercise.Id, savedSolution.ExerciseId);
        Assert.Equal(solutionCode, savedSolution.Code);
        Assert.Null(savedSolution.LastCheckedAt);

        var loadedSolution = await client.GetFromJsonAsync<CodingExerciseSolutionResponse>(
            $"/api/catalog/topics/{topicSlug}/exercises/{exercise.Id}/solution",
            JsonOptions);
        Assert.NotNull(loadedSolution);
        Assert.Equal(solutionCode, loadedSolution.Code);

        var checkResponse = await client.PostAsJsonAsync(
            $"/api/catalog/topics/{topicSlug}/exercises/{exercise.Id}/check",
            new AnalyzeExerciseCodeRequest(solutionCode),
            JsonOptions);
        checkResponse.EnsureSuccessStatusCode();

        var check = await checkResponse.Content.ReadFromJsonAsync<ExerciseCheckResponse>(JsonOptions);
        Assert.NotNull(check);
        Assert.True(check.Succeeded);
        Assert.Empty(check.Diagnostics);
        Assert.Equal("passed", check.Grading.Status);
        Assert.Equal(0, check.Grading.RequiredFailed);
        Assert.Contains(check.Grading.Checks, rule => rule is { Id: "behavior.cancellation", Status: "pass" });
        Assert.Contains(check.Grading.SampleCases, sample => sample.Name == "trims input");
        Assert.Contains(check.PackageRequirements, package => package is { Manager: "nuget", Name: "xunit", Status: "declared" });
        Assert.Contains(check.PackageRequirements, package => package is { Manager: "npm", Name: "none", Status: "notRequired" });

        var brokenDiagnosticsResponse = await client.PostAsJsonAsync(
            $"/api/catalog/topics/{topicSlug}/exercises/{exercise.Id}/diagnostics",
            new AnalyzeExerciseCodeRequest("public static class Exercise { public static string Run() => Missing.Symbol; }"),
            JsonOptions);
        brokenDiagnosticsResponse.EnsureSuccessStatusCode();

        var brokenDiagnostics = await brokenDiagnosticsResponse.Content.ReadFromJsonAsync<ExerciseCheckResponse>(JsonOptions);
        Assert.NotNull(brokenDiagnostics);
        Assert.False(brokenDiagnostics.Succeeded);
        Assert.Contains(brokenDiagnostics.Diagnostics, diagnostic => diagnostic.Severity == "error");

        var semanticFailureResponse = await client.PostAsJsonAsync(
            $"/api/catalog/topics/{topicSlug}/exercises/{exercise.Id}/check",
            new AnalyzeExerciseCodeRequest("""
                using System.Threading;
                using System.Threading.Tasks;

                public static class Exercise
                {
                    public static Task<string> RunAsync(string input, CancellationToken cancellationToken = default)
                    {
                        return Task.FromResult(input.Trim());
                    }
                }
                """),
            JsonOptions);
        semanticFailureResponse.EnsureSuccessStatusCode();

        var semanticFailure = await semanticFailureResponse.Content.ReadFromJsonAsync<ExerciseCheckResponse>(JsonOptions);
        Assert.NotNull(semanticFailure);
        Assert.False(semanticFailure.Succeeded);
        Assert.Empty(semanticFailure.Diagnostics);
        Assert.Equal("needsWork", semanticFailure.Grading.Status);
        Assert.Contains(semanticFailure.Grading.Checks, rule => rule is { Id: "behavior.invalid-input", Status: "fail" });
        Assert.Contains(semanticFailure.Grading.Checks, rule => rule is { Id: "behavior.cancellation", Status: "fail" });
    }

    [Fact]
    public async Task Study_item_flow_generates_saves_and_lists_items_for_source_chunk()
    {
        using var client = _factory.CreateClient();
        var chunkId = await SeedSourceChunkAsync();

        var generatedResponse = await client.PostAsJsonAsync(
            "/api/study-items/generate",
            new GenerateStudyItemsRequest(chunkId),
            JsonOptions);
        generatedResponse.EnsureSuccessStatusCode();

        var generated = await generatedResponse.Content.ReadFromJsonAsync<GeneratedStudyItemsResponse>(JsonOptions);
        Assert.NotNull(generated);
        Assert.NotEmpty(generated.Items);
        Assert.Contains(generated.Items, item => item.Kind is StudyItemKind.CodeReading or StudyItemKind.CodingExercise);

        var selected = generated.Items.First();
        var saveResponse = await client.PostAsJsonAsync(
            "/api/study-items/",
            new CreateStudyItemsRequest(
                chunkId,
                null,
                [
                    new CreateStudyItemRequest(
                        selected.Kind,
                        selected.Prompt,
                        selected.ExpectedAnswer,
                        selected.Explanation,
                        selected.SourceExcerpt)
                ]),
            JsonOptions);
        saveResponse.EnsureSuccessStatusCode();

        var saved = await saveResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<StudyItemResponse>>(JsonOptions);
        Assert.NotNull(saved);
        var savedItem = Assert.Single(saved);
        Assert.Equal(chunkId, savedItem.SourceDocumentChunkId);
        Assert.Equal(StudyItemStatus.Active, savedItem.Status);

        var listResponse = await client.GetAsync($"/api/study-items/?sourceDocumentChunkId={chunkId}");
        var listBody = await listResponse.Content.ReadAsStringAsync();
        Assert.True(listResponse.IsSuccessStatusCode, listBody);

        var listed = JsonSerializer.Deserialize<IReadOnlyCollection<StudyItemResponse>>(listBody, JsonOptions);
        Assert.NotNull(listed);
        Assert.Single(listed);
    }

    private async Task<int> SeedSourceChunkAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyPlatformDbContext>();

        var source = new SourceMaterial(
            $"test-source-{Guid.NewGuid():N}",
            "JavaScript Test Source",
            "Test Author",
            "test.pdf",
            "local-sources/pdfs/test.pdf",
            SourceAccess.PurchasedLocal,
            1024);
        source.SyncMetadata(
            "JavaScript Test Source",
            "Test Author",
            "test.pdf",
            "local-sources/pdfs/test.pdf",
            SourceAccess.PurchasedLocal,
            1024,
            12);

        db.SourceMaterials.Add(source);
        await db.SaveChangesAsync();

        var chunk = new SourceDocumentChunk(
            source.Id,
            1,
            1,
            4,
            "Functions and scope",
            """
            Functions create a new scope. This means variables declared inside the function are not available outside that function.

            function doingStuff() {
            if (true) {
            var x = "local";
            }
            console.log(x);
            }

            doingStuff();

            When var is used inside a function, the variable is function-scoped. This is why the example can still access x after the if block.
            """);

        db.SourceDocumentChunks.Add(chunk);
        await db.SaveChangesAsync();

        return chunk.Id;
    }

    private async Task<string> SeedTopicAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyPlatformDbContext>();

        var module = new CurriculumModule(
            $"module-{Guid.NewGuid():N}",
            $"module-{Guid.NewGuid():N}",
            "ASP.NET Core API Engineering",
            1,
            "Backend API topics.");
        db.Modules.Add(module);
        await db.SaveChangesAsync();

        var topicSlug = $"minimal-api-routing-{Guid.NewGuid():N}";
        var topic = new Topic(
            $"topic-{Guid.NewGuid():N}",
            topicSlug,
            "Minimal API route groups",
            module.Id,
            1,
            "Design stable route groups with typed results and cancellation-aware handlers.");
        db.Topics.Add(topic);
        await db.SaveChangesAsync();

        return topicSlug;
    }

    public void Dispose()
    {
        _factory.Dispose();

        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }

        if (File.Exists($"{_databasePath}-wal"))
        {
            File.Delete($"{_databasePath}-wal");
        }

        if (File.Exists($"{_databasePath}-shm"))
        {
            File.Delete($"{_databasePath}-shm");
        }
    }
}
