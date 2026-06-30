using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFSSPlatform.Api.Features.LearningResources;
using SFSSPlatform.Infrastructure.Persistence;
using SFSSPlatform.Infrastructure.StudyContent;

namespace SFSSPlatform.Tests.LearningResources;

public sealed class LearningResourceApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"sfssplatform-learning-resource-tests-{Guid.NewGuid():N}.db");

    private readonly WebApplicationFactory<Program> _factory;

    public LearningResourceApiTests()
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
    public async Task Learning_resource_flow_imports_lists_and_saves_video_notes()
    {
        using var client = _factory.CreateClient();

        var importResponse = await client.PostAsync("/api/learning-resources/import-seed", null);
        importResponse.EnsureSuccessStatusCode();

        var importResult = await importResponse.Content.ReadFromJsonAsync<LearningResourceImportResult>(JsonOptions);
        Assert.NotNull(importResult);
        Assert.True(importResult.ResourcesDiscovered >= 6);
        Assert.True(importResult.ResourcesCreated >= 6);

        var listResponse = await client.GetAsync("/api/learning-resources/?tag=dotnet");
        var listBody = await listResponse.Content.ReadAsStringAsync();
        Assert.True(listResponse.IsSuccessStatusCode, listBody);

        var resources = JsonSerializer.Deserialize<IReadOnlyCollection<LearningResourceResponse>>(listBody, JsonOptions);
        Assert.NotNull(resources);
        var dotnetCourse = Assert.Single(resources, resource => resource.ExternalId == "YbRe4iIVYJk");
        Assert.Equal("https://www.youtube-nocookie.com/embed/YbRe4iIVYJk", dotnetCourse.EmbedUrl);
        Assert.Contains("dotnet", dotnetCourse.Tags);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/learning-resources/{dotnetCourse.Id}/watch-state",
            new UpdateLearningResourceWatchStateRequest(true, 600, "MVC setup and app structure are worth revisiting."),
            JsonOptions);
        updateResponse.EnsureSuccessStatusCode();

        var updated = await updateResponse.Content.ReadFromJsonAsync<LearningResourceResponse>(JsonOptions);
        Assert.NotNull(updated);
        Assert.True(updated.IsWatched);
        Assert.Equal(600, updated.WatchProgressSeconds);
        Assert.Equal("MVC setup and app structure are worth revisiting.", updated.Notes);
        Assert.NotNull(updated.WatchedAt);
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
