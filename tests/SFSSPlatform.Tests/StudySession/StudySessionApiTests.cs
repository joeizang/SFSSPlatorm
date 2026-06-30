using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFSSPlatform.Api.Features.StudySession;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Tests.StudySession;

public sealed class StudySessionApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"sfssplatform-study-session-tests-{Guid.NewGuid():N}.db");

    private readonly WebApplicationFactory<Program> _factory;

    public StudySessionApiTests()
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
            });
    }

    [Fact]
    public async Task Study_session_returns_due_item_and_records_attempt()
    {
        using var client = _factory.CreateClient();
        await SeedStudyItemAsync();

        var nextResponse = await client.GetAsync("/api/study-session/next");
        var nextBody = await nextResponse.Content.ReadAsStringAsync();
        Assert.True(nextResponse.IsSuccessStatusCode, nextBody);
        var next = JsonSerializer.Deserialize<StudySessionNextResponse>(nextBody, JsonOptions);
        Assert.NotNull(next);
        Assert.NotNull(next.Item);
        Assert.Equal("What does ASP.NET Core middleware do?", next.Item.Prompt);
        Assert.Equal(1, next.DueCount);

        var attemptResponse = await client.PostAsJsonAsync(
            "/api/study-session/attempts",
            new RecordStudyAttemptRequest(next.Item.Id, "It creates a request pipeline.", StudyAttemptRating.Good),
            JsonOptions);
        attemptResponse.EnsureSuccessStatusCode();

        var attempt = await attemptResponse.Content.ReadFromJsonAsync<StudyAttemptResponse>(JsonOptions);
        Assert.NotNull(attempt);
        Assert.Equal(1, attempt.AttemptCount);
        Assert.True(attempt.ConfidenceScore > 0);
        Assert.NotNull(attempt.NextReviewAt);

        var summary = await client.GetFromJsonAsync<StudySessionSummaryResponse>("/api/study-session/summary", JsonOptions);
        Assert.NotNull(summary);
        Assert.Equal(1, summary.ActiveItems);
        Assert.Equal(1, summary.AnsweredToday);
    }

    private async Task<int> SeedStudyItemAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyPlatformDbContext>();

        var source = new SourceMaterial(
            $"session-source-{Guid.NewGuid():N}",
            "ASP.NET Core Test Source",
            "Test Author",
            "test.pdf",
            "local-sources/pdfs/test.pdf",
            SourceAccess.PurchasedLocal,
            1024);
        source.SyncMetadata(
            "ASP.NET Core Test Source",
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
            "Middleware",
            "Middleware forms a request pipeline for handling HTTP requests.");
        db.SourceDocumentChunks.Add(chunk);
        await db.SaveChangesAsync();

        var item = new StudyItem(
            source.Id,
            chunk.Id,
            StudyItemKind.ShortAnswer,
            "What does ASP.NET Core middleware do?",
            "It composes a request pipeline that handles HTTP requests and responses.",
            "This checks the role of middleware in the HTTP pipeline.",
            "Middleware forms a request pipeline for handling HTTP requests.",
            1,
            4,
            DateTimeOffset.UtcNow);
        db.StudyItems.Add(item);
        await db.SaveChangesAsync();

        return item.Id;
    }

    public void Dispose()
    {
        _factory.Dispose();

        foreach (var path in new[] { _databasePath, $"{_databasePath}-wal", $"{_databasePath}-shm" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
