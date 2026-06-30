using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed class LearningResourceSeeder(StudyPlatformDbContext db, TimeProvider timeProvider)
{
    private static readonly IReadOnlyCollection<LearningResourceSeed> Seeds =
    [
        new(
            "2EGzAPhz2nE",
            LearningResourceProvider.YouTube,
            "Turbocharged: Writing High-Performance C# and .NET Code",
            "NDC Conferences",
            "https://www.youtube.com/watch?v=2EGzAPhz2nE",
            "https://www.youtube-nocookie.com/embed/2EGzAPhz2nE",
            "Steve Gordon's NDC talk on writing faster .NET code with fewer allocations, covering profiling, benchmarking, Span, pools, pipelines, and low-level APIs.",
            "advanced,dotnet,csharp,performance,memory,benchmarking,backend",
            null),
        new(
            "s5xNMWkYPwQ",
            LearningResourceProvider.YouTube,
            "Writing (in)efficient C#",
            "NDC Conferences",
            "https://www.youtube.com/watch?v=s5xNMWkYPwQ",
            "https://www.youtube-nocookie.com/embed/s5xNMWkYPwQ",
            "Callum Whyte's NDC Porto talk on recognizing inefficient C# patterns and improving runtime behavior in production code.",
            "advanced,dotnet,csharp,performance,backend,code-quality",
            null),
        new(
            "Qn9oCsMJN-c",
            LearningResourceProvider.YouTube,
            "10 things you didn't know EF Core can do",
            "NDC Conferences",
            "https://www.youtube.com/watch?v=Qn9oCsMJN-c",
            "https://www.youtube-nocookie.com/embed/Qn9oCsMJN-c",
            "Hannes Lowette's NDC Oslo session on lesser-known EF Core capabilities that help with real-world data access and persistence design.",
            "advanced,dotnet,ef-core,database,persistence,backend",
            null),
        new(
            "YfIM-gfJe4c",
            LearningResourceProvider.YouTube,
            ".NET Data Community Standup - Database concurrency and EF Core",
            ".NET",
            "https://www.youtube.com/watch?v=YfIM-gfJe4c",
            "https://www.youtube-nocookie.com/embed/YfIM-gfJe4c",
            "Official .NET community standup covering database concurrency and optimistic concurrency patterns with EF Core.",
            "advanced,dotnet,ef-core,database,concurrency,persistence,official",
            null),
        new(
            "fc6_NtD9soI",
            LearningResourceProvider.YouTube,
            "Modularizing the Monolith",
            "NDC Conferences",
            "https://www.youtube.com/watch?v=fc6_NtD9soI",
            "https://www.youtube-nocookie.com/embed/fc6_NtD9soI",
            "Jimmy Bogard's NDC Oslo talk on moving beyond technical-layer organization toward modular boundaries that can evolve cleanly.",
            "advanced,architecture,modular-monolith,dotnet,backend,domain-design",
            null),
        new(
            "mT5bhj1Wygg",
            LearningResourceProvider.YouTube,
            "Design more decoupled services with one weird trick",
            "NDC Conferences",
            "https://www.youtube.com/watch?v=mT5bhj1Wygg",
            "https://www.youtube-nocookie.com/embed/mT5bhj1Wygg",
            "Udi Dahan's NDC Oslo session on service boundaries, coupling, and distributed-system design tradeoffs.",
            "advanced,architecture,distributed-systems,microservices,backend,domain-design",
            null),
        new(
            "qcJASFx-F5g",
            LearningResourceProvider.YouTube,
            "So You Want to Build An Event Driven System?",
            "NDC Conferences",
            "https://www.youtube.com/watch?v=qcJASFx-F5g",
            "https://www.youtube-nocookie.com/embed/qcJASFx-F5g",
            "James Eastham's NDC London talk on the benefits, challenges, coupling, and tradeoffs involved in event-driven architecture.",
            "advanced,architecture,event-driven,distributed-systems,messaging,backend",
            null),
        new(
            "pOo7x8OiAec",
            LearningResourceProvider.YouTube,
            "And Now You Understand React Server Components",
            "React Conf",
            "https://www.youtube.com/watch?v=pOo7x8OiAec",
            "https://www.youtube-nocookie.com/embed/pOo7x8OiAec",
            "Kent C. Dodds' React Conf talk explaining React Server Components and the mental model changes required for modern React apps.",
            "advanced,react,frontend,server-components,architecture",
            null),
        new(
            "kjOacmVsLSE",
            LearningResourceProvider.YouTube,
            "React for developers and compilers",
            "React India",
            "https://www.youtube.com/watch?v=kjOacmVsLSE",
            "https://www.youtube-nocookie.com/embed/kjOacmVsLSE",
            "Sathya Gunasekaran's talk on React Forget and compiler-assisted React optimization.",
            "advanced,react,frontend,compiler,performance",
            null),
        new(
            "T-rHmWSZajc",
            LearningResourceProvider.YouTube,
            "Nadia Makarevich - How React Compiler Performs on Real Code, React Advanced 2024",
            "React Conferences by GitNation",
            "https://www.youtube.com/watch?v=T-rHmWSZajc",
            "https://www.youtube-nocookie.com/embed/T-rHmWSZajc",
            "Nadia Makarevich examines React Compiler behavior on real applications, useful for understanding practical optimization impact.",
            "advanced,react,frontend,compiler,performance",
            null),
        new(
            "P-J9Eg7hJwE",
            LearningResourceProvider.YouTube,
            "Adopting Typescript at Scale",
            "JSConf",
            "https://www.youtube.com/watch?v=P-J9Eg7hJwE",
            "https://www.youtube-nocookie.com/embed/P-J9Eg7hJwE",
            "Brie Bunge's JSConf Hawaii talk on Airbnb's TypeScript migration, focusing on adoption strategy, scale, and migration tradeoffs.",
            "advanced,typescript,frontend,migration,large-codebase,engineering-practice",
            null),
        new(
            "cCOL7MC4Pl0",
            LearningResourceProvider.YouTube,
            "Jake Archibald on the web browser event loop, setTimeout, micro tasks, requestAnimationFrame, ...",
            "JSConf",
            "https://www.youtube.com/watch?v=cCOL7MC4Pl0",
            "https://www.youtube-nocookie.com/embed/cCOL7MC4Pl0",
            "Jake Archibald's deep dive into the browser event loop, tasks, microtasks, rendering, and scheduling behavior.",
            "advanced,javascript,frontend,browser,event-loop,performance",
            null),
        new(
            "8aGhZQkoFbQ",
            LearningResourceProvider.YouTube,
            "What the heck is the event loop anyway?",
            "JSConf",
            "https://www.youtube.com/watch?v=8aGhZQkoFbQ",
            "https://www.youtube-nocookie.com/embed/8aGhZQkoFbQ",
            "Philip Roberts' JSConf EU talk explaining JavaScript runtime behavior, callbacks, queues, and non-blocking execution.",
            "javascript,frontend,event-loop,async,runtime",
            null),
        new(
            "YbRe4iIVYJk",
            LearningResourceProvider.YouTube,
            "ASP.NET Core Full Course For Beginners (.NET 10)",
            "Julio Casal",
            "https://www.youtube.com/watch?v=YbRe4iIVYJk",
            "https://www.youtube-nocookie.com/embed/YbRe4iIVYJk",
            "Broad ASP.NET Core course kept as a full-course reference for filling practical web API and application-development gaps.",
            "dotnet,csharp,aspnet-core,backend,full-course,fundamentals",
            null),
        new(
            "30LWjhZzg50",
            LearningResourceProvider.YouTube,
            "Learn TypeScript - Full Tutorial",
            "freeCodeCamp.org",
            "https://www.youtube.com/watch?v=30LWjhZzg50",
            "https://www.youtube-nocookie.com/embed/30LWjhZzg50",
            "Full TypeScript tutorial from freeCodeCamp kept as a broad reference for filling language gaps before advanced TypeScript work.",
            "typescript,javascript,frontend,language,fundamentals",
            null)
    ];

    private static readonly string[] RetiredUntouchedSeedExternalIds =
    [
        "SryQxUeChMc",
        "FJDVKeh7RJI",
        "PkZNo7MFNFg",
        "lfmg-EJ8gm4"
    ];

    public async Task<LearningResourceImportResult> ImportCuratedVideosAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var created = 0;
        var updated = 0;

        foreach (var seed in Seeds)
        {
            var resource = await db.LearningResources
                .SingleOrDefaultAsync(
                    item => item.Provider == seed.Provider && item.ExternalId == seed.ExternalId,
                    cancellationToken);

            if (resource is null)
            {
                db.LearningResources.Add(new LearningResource(
                    seed.ExternalId,
                    seed.Provider,
                    seed.Title,
                    seed.Creator,
                    seed.Url,
                    seed.EmbedUrl,
                    seed.Summary,
                    seed.Tags,
                    seed.DurationSeconds,
                    now));
                created++;
                continue;
            }

            resource.SyncMetadata(
                seed.Title,
                seed.Creator,
                seed.Url,
                seed.EmbedUrl,
                seed.Summary,
                seed.Tags,
                seed.DurationSeconds,
                now);
            updated++;
        }

        await PruneRetiredUntouchedSeedsAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return new LearningResourceImportResult(Seeds.Count, created, updated);
    }

    private async Task PruneRetiredUntouchedSeedsAsync(CancellationToken cancellationToken)
    {
        var retired = await db.LearningResources
            .Where(resource =>
                resource.Provider == LearningResourceProvider.YouTube
                && RetiredUntouchedSeedExternalIds.Contains(resource.ExternalId)
                && !resource.IsWatched
                && resource.WatchProgressSeconds == 0
                && resource.Notes == string.Empty
                && resource.SourceMaterialId == null
                && resource.SourceDocumentChunkId == null)
            .ToListAsync(cancellationToken);

        db.LearningResources.RemoveRange(retired);
    }
}

public sealed record LearningResourceImportResult(int ResourcesDiscovered, int ResourcesCreated, int ResourcesUpdated);
