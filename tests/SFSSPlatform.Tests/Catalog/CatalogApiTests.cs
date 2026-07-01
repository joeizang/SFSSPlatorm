using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFSSPlatform.Api.Features.Catalog;
using SFSSPlatform.Infrastructure.Persistence;
using SFSSPlatform.Domain.Curriculum;
using SFSSPlatform.Infrastructure.Curriculum;
using TaskType = SFSSPlatform.Domain.Curriculum.TaskType;

namespace SFSSPlatform.Tests.Catalog;

public sealed class CatalogApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"sfssplatform-tests-{Guid.NewGuid():N}.db");

    private readonly WebApplicationFactory<Program> _factory;

    public CatalogApiTests()
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
    public async Task Catalog_flow_imports_curriculum_updates_progress_and_returns_rollups()
    {
        using var client = _factory.CreateClient();

        var importResponse = await client.PostAsJsonAsync(
            "/api/curriculum/import",
            TestCurriculumSeed.Create(),
            JsonOptions);
        importResponse.EnsureSuccessStatusCode();

        var topics = await client.GetFromJsonAsync<IReadOnlyCollection<TopicSearchResponse>>(
            "/api/catalog/topics",
            JsonOptions);
        Assert.NotNull(topics);
        Assert.Equal(3, topics.Count);
        Assert.Contains(topics, topic => topic.Slug == "minimal-api-routing");

        var progressResponse = await client.PutAsJsonAsync(
            "/api/catalog/topics/minimal-api-routing/progress",
            new UpdateTopicProgressRequest(TopicProgressStatus.Done),
            JsonOptions);
        progressResponse.EnsureSuccessStatusCode();

        var topic = await client.GetFromJsonAsync<TopicDetailResponse>(
            "/api/catalog/topics/minimal-api-routing",
            JsonOptions);
        Assert.NotNull(topic);
        Assert.Equal(TopicProgressStatus.Done, topic.Status);
        Assert.NotNull(topic.StartedAt);
        Assert.NotNull(topic.CompletedAt);

        var rollups = await client.GetFromJsonAsync<CatalogRollupsResponse>(
            "/api/catalog/rollups",
            JsonOptions);
        Assert.NotNull(rollups);

        var apiModule = Assert.Single(rollups.Modules, module => module.Slug == "aspnet-core");
        Assert.Equal(2, apiModule.TotalTopics);
        Assert.Equal(1, apiModule.DoneTopics);
        Assert.Equal(50m, apiModule.CompletionPercentage);
    }

    [Fact]
    public async Task Missing_topic_returns_not_found()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/catalog/topics/not-a-topic");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Topic_notes_can_be_read_and_saved_for_a_single_topic()
    {
        using var client = _factory.CreateClient();

        var importResponse = await client.PostAsJsonAsync(
            "/api/curriculum/import",
            TestCurriculumSeed.Create(),
            JsonOptions);
        importResponse.EnsureSuccessStatusCode();

        var emptyNote = await client.GetFromJsonAsync<TopicNoteResponse>(
            "/api/catalog/topics/minimal-api-routing/notes/",
            JsonOptions);
        Assert.NotNull(emptyNote);
        Assert.Equal("minimal-api-routing", emptyNote.TopicSlug);
        Assert.Equal(string.Empty, emptyNote.Content);

        var saveResponse = await client.PutAsJsonAsync(
            "/api/catalog/topics/minimal-api-routing/notes/",
            new UpsertTopicNoteRequest("Route groups should be stable contracts, not folders."),
            JsonOptions);
        saveResponse.EnsureSuccessStatusCode();

        var savedNote = await saveResponse.Content.ReadFromJsonAsync<TopicNoteResponse>(JsonOptions);
        Assert.NotNull(savedNote);
        Assert.Equal("Route groups should be stable contracts, not folders.", savedNote.Content);

        var reloadedNote = await client.GetFromJsonAsync<TopicNoteResponse>(
            "/api/catalog/topics/minimal-api-routing/notes/",
            JsonOptions);
        Assert.NotNull(reloadedNote);
        Assert.Equal(savedNote.Content, reloadedNote.Content);
    }

    public void Dispose()
    {
        _factory.Dispose();

        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
