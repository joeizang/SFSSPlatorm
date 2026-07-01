using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFSSPlatform.Api.Features.LearningResources;
using SFSSPlatform.Domain.Curriculum;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Tests.LearningResources;

public sealed class TopicResourceLinkApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"sfssplatform-topic-resource-tests-{Guid.NewGuid():N}.db");

    private readonly WebApplicationFactory<Program> _factory;

    public TopicResourceLinkApiTests()
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
    public async Task Topic_resource_flow_links_lists_unlinked_and_deletes_resources()
    {
        using var client = _factory.CreateClient();
        var seed = await SeedAsync();

        var resourceLinkResponse = await client.PostAsJsonAsync(
            "/api/topic-resource-links/",
            new UpsertTopicResourceLinkRequest(
                seed.TopicSlug,
                seed.LearningResourceId,
                null,
                5,
                "Best video before exercises."),
            JsonOptions);
        resourceLinkResponse.EnsureSuccessStatusCode();

        var resourceLink = await resourceLinkResponse.Content.ReadFromJsonAsync<TopicResourceLinkResponse>(JsonOptions);
        Assert.NotNull(resourceLink);
        Assert.Equal(seed.TopicSlug, resourceLink.TopicSlug);
        Assert.Equal(seed.LearningResourceId, resourceLink.LearningResourceId);
        Assert.Equal("Deep .NET Async", resourceLink.Title);
        Assert.Equal(5, resourceLink.Priority);

        var candidateLinkResponse = await client.PostAsJsonAsync(
            "/api/topic-resource-links/",
            new UpsertTopicResourceLinkRequest(
                seed.TopicSlug,
                null,
                seed.VideoCandidateId,
                4,
                "Candidate for the same topic."),
            JsonOptions);
        candidateLinkResponse.EnsureSuccessStatusCode();

        var links = await client.GetFromJsonAsync<IReadOnlyCollection<TopicResourceLinkResponse>>(
            $"/api/topic-resource-links/?topicSlug={seed.TopicSlug}",
            JsonOptions);
        Assert.NotNull(links);
        Assert.Equal(2, links.Count);
        Assert.Contains(links, link => link.VideoCandidateId == seed.VideoCandidateId);

        var unlinked = await client.GetFromJsonAsync<UnlinkedTopicResourcesResponse>(
            "/api/topic-resource-links/unlinked",
            JsonOptions);
        Assert.NotNull(unlinked);
        Assert.DoesNotContain(unlinked.LearningResources, resource => resource.Id == seed.LearningResourceId);
        Assert.DoesNotContain(unlinked.VideoCandidates, candidate => candidate.Id == seed.VideoCandidateId);

        var deleteResponse = await client.DeleteAsync($"/api/topic-resource-links/{resourceLink.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var remaining = await client.GetFromJsonAsync<IReadOnlyCollection<TopicResourceLinkResponse>>(
            $"/api/topic-resource-links/?topicSlug={seed.TopicSlug}",
            JsonOptions);
        Assert.NotNull(remaining);
        Assert.Single(remaining);
    }

    private async Task<SeedResult> SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyPlatformDbContext>();
        var now = DateTimeOffset.UtcNow;

        var module = new CurriculumModule(
            $"module-{Guid.NewGuid():N}",
            $"module-{Guid.NewGuid():N}",
            "Backend Engineering",
            1,
            "Backend topics.");
        db.Modules.Add(module);
        await db.SaveChangesAsync();

        var topicSlug = $"async-runtime-{Guid.NewGuid():N}";
        var topic = new Topic(
            $"topic-{Guid.NewGuid():N}",
            topicSlug,
            "Async runtime",
            module.Id,
            1,
            "Understand async internals.");
        db.Topics.Add(topic);

        var resource = new LearningResource(
            "R-z2Hv-7nxk",
            LearningResourceProvider.YouTube,
            "Deep .NET Async",
            "dotnet",
            "https://www.youtube.com/watch?v=R-z2Hv-7nxk",
            "https://www.youtube-nocookie.com/embed/R-z2Hv-7nxk",
            "Async internals with Stephen Toub.",
            "dotnet,csharp,async",
            null,
            now);
        db.LearningResources.Add(resource);

        var candidate = new VideoCandidate(
            "18w4QOWGJso",
            "Deep .NET Parallel Programming",
            "dotnet",
            "https://www.youtube.com/@dotnet",
            "https://www.youtube.com/watch?v=18w4QOWGJso",
            "https://www.youtube-nocookie.com/embed/18w4QOWGJso",
            "Parallel programming in .NET.",
            "dotnet,csharp,parallel-programming",
            VideoCandidateDifficulty.Expert,
            "Candidate for concurrency study.",
            now);
        db.VideoCandidates.Add(candidate);

        await db.SaveChangesAsync();
        return new SeedResult(topicSlug, resource.Id, candidate.Id);
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

public sealed record SeedResult(string TopicSlug, int LearningResourceId, int VideoCandidateId);
