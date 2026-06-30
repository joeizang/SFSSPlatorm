using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFSSPlatform.Api.Features.StudyItems;
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
