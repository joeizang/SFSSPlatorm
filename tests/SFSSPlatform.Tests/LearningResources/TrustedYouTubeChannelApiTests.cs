using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFSSPlatform.Api.Features.LearningResources;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;
using SFSSPlatform.Infrastructure.StudyContent;

namespace SFSSPlatform.Tests.LearningResources;

public sealed class TrustedYouTubeChannelApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"sfssplatform-trusted-channel-tests-{Guid.NewGuid():N}.db");

    private readonly string _channelFilePath = Path.Combine(
        Path.GetTempPath(),
        $"sfssplatform-trusted-channels-{Guid.NewGuid():N}.csv");

    private readonly WebApplicationFactory<Program> _factory;

    public TrustedYouTubeChannelApiTests()
    {
        File.WriteAllText(
            _channelFilePath,
            """
            channelName,channelUrl,tags,priority,notes
            Nick Chapsas,https://www.youtube.com/@nickchapsas,dotnet;csharp;architecture,High,Modern .NET and architecture.
            Raw Coding,https://www.youtube.com/@RawCoding,dotnet;aspnet-core;csharp,High,ASP.NET Core implementation depth.
            Beginner Channel,https://www.youtube.com/@beginner,dotnet,Medium,Useful but lower priority.
            """);

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Default"] = $"Data Source={_databasePath}",
                        ["LocalYouTubeChannels:SourceFile"] = _channelFilePath
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
    public async Task Trusted_channel_flow_imports_local_csv_and_lists_channels()
    {
        using var client = _factory.CreateClient();

        var sourceFile = await client.GetFromJsonAsync<TrustedYouTubeChannelSourceFileResponse>(
            "/api/learning-resources/channels/source-file",
            JsonOptions);
        Assert.NotNull(sourceFile);
        Assert.Equal(_channelFilePath, sourceFile.SourceFile);

        var importResponse = await client.PostAsync("/api/learning-resources/channels/import-local", null);
        importResponse.EnsureSuccessStatusCode();

        var importResult = await importResponse.Content.ReadFromJsonAsync<TrustedYouTubeChannelImportResult>(JsonOptions);
        Assert.NotNull(importResult);
        Assert.Equal(3, importResult.ChannelsDiscovered);
        Assert.Equal(3, importResult.ChannelsCreated);
        Assert.Equal(0, importResult.ChannelsSkipped);

        var listResponse = await client.GetAsync("/api/learning-resources/channels/?tag=dotnet");
        var listBody = await listResponse.Content.ReadAsStringAsync();
        Assert.True(listResponse.IsSuccessStatusCode, listBody);

        var channels = JsonSerializer.Deserialize<IReadOnlyCollection<TrustedYouTubeChannelResponse>>(listBody, JsonOptions);
        Assert.NotNull(channels);
        Assert.Equal(3, channels.Count);
        Assert.Equal("Nick Chapsas", channels.First().Name);
        Assert.Contains(channels, channel => channel.Name == "Nick Chapsas" && channel.Priority == TrustedChannelPriority.High);
        Assert.Contains(channels, channel => channel.Name == "Raw Coding" && channel.Tags.Contains("aspnet-core"));
    }

    public void Dispose()
    {
        _factory.Dispose();

        foreach (var path in new[] { _databasePath, $"{_databasePath}-wal", $"{_databasePath}-shm", _channelFilePath })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
