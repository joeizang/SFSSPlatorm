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

public sealed class VideoCandidateApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"sfssplatform-video-candidate-tests-{Guid.NewGuid():N}.db");

    private readonly string _candidateFilePath = Path.Combine(
        Path.GetTempPath(),
        $"sfssplatform-video-candidates-{Guid.NewGuid():N}.csv");

    private readonly WebApplicationFactory<Program> _factory;

    public VideoCandidateApiTests()
    {
        File.WriteAllText(
            _candidateFilePath,
            """
            title,channelName,channelUrl,videoUrl,tags,difficulty,summary,notes
            Deep .NET Async,dotnet,https://www.youtube.com/@dotnet,https://www.youtube.com/watch?v=R-z2Hv-7nxk,dotnet;csharp;async,Expert,Async internals with Stephen Toub.,High-value runtime candidate.
            React Performance,Web Dev Simplified,https://www.youtube.com/@WebDevSimplified,https://www.youtube.com/watch?v=Qwb-Za6cBws,react;performance;frontend,Intermediate,React performance techniques.,Useful frontend reference.
            """);

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Default"] = $"Data Source={_databasePath}",
                        ["LocalVideoCandidates:SourceFile"] = _candidateFilePath
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
    public async Task Video_candidate_flow_imports_accepts_and_rejects_candidates()
    {
        using var client = _factory.CreateClient();

        var sourceFile = await client.GetFromJsonAsync<VideoCandidateSourceFileResponse>(
            "/api/learning-resources/candidates/source-file",
            JsonOptions);
        Assert.NotNull(sourceFile);
        Assert.Equal(_candidateFilePath, sourceFile.SourceFile);

        var importResponse = await client.PostAsync("/api/learning-resources/candidates/import-local", null);
        importResponse.EnsureSuccessStatusCode();

        var importResult = await importResponse.Content.ReadFromJsonAsync<VideoCandidateImportResult>(JsonOptions);
        Assert.NotNull(importResult);
        Assert.Equal(2, importResult.CandidatesDiscovered);
        Assert.Equal(2, importResult.CandidatesCreated);

        var candidates = await client.GetFromJsonAsync<IReadOnlyCollection<VideoCandidateResponse>>(
            "/api/learning-resources/candidates/?status=candidate",
            JsonOptions);
        Assert.NotNull(candidates);
        Assert.Equal(2, candidates.Count);

        var asyncCandidate = Assert.Single(candidates, candidate => candidate.ExternalId == "R-z2Hv-7nxk");
        var acceptResponse = await client.PostAsync($"/api/learning-resources/candidates/{asyncCandidate.Id}/accept", null);
        acceptResponse.EnsureSuccessStatusCode();

        var accepted = await acceptResponse.Content.ReadFromJsonAsync<AcceptVideoCandidateResponse>(JsonOptions);
        Assert.NotNull(accepted);
        Assert.Equal(VideoCandidateStatus.Accepted, accepted.Candidate.Status);
        Assert.Equal("Deep .NET Async", accepted.Resource.Title);
        Assert.Equal("https://www.youtube-nocookie.com/embed/R-z2Hv-7nxk", accepted.Resource.EmbedUrl);

        var resources = await client.GetFromJsonAsync<IReadOnlyCollection<LearningResourceResponse>>(
            "/api/learning-resources/?search=Deep%20.NET%20Async",
            JsonOptions);
        Assert.NotNull(resources);
        Assert.Single(resources, resource => resource.ExternalId == "R-z2Hv-7nxk");

        var reactCandidate = Assert.Single(candidates, candidate => candidate.ExternalId == "Qwb-Za6cBws");
        var rejectResponse = await client.PostAsJsonAsync(
            $"/api/learning-resources/candidates/{reactCandidate.Id}/reject",
            new RejectVideoCandidateRequest("Already covered by another React resource."),
            JsonOptions);
        rejectResponse.EnsureSuccessStatusCode();

        var rejected = await rejectResponse.Content.ReadFromJsonAsync<VideoCandidateResponse>(JsonOptions);
        Assert.NotNull(rejected);
        Assert.Equal(VideoCandidateStatus.Rejected, rejected.Status);
        Assert.Equal("Already covered by another React resource.", rejected.RejectionReason);
    }

    public void Dispose()
    {
        _factory.Dispose();

        foreach (var path in new[] { _databasePath, $"{_databasePath}-wal", $"{_databasePath}-shm", _candidateFilePath })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
