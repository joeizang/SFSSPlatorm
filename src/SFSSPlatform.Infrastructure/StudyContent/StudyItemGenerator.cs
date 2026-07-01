using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed partial class StudyItemGenerator(StudyPlatformDbContext db)
{
    public async Task<IReadOnlyCollection<StudyItemDraft>> GenerateForChunkAsync(
        int chunkId,
        CancellationToken cancellationToken)
    {
        var chunk = await db.SourceDocumentChunks
            .AsNoTracking()
            .Include(sourceChunk => sourceChunk.SourceMaterial)
            .SingleAsync(sourceChunk => sourceChunk.Id == chunkId, cancellationToken);

        var text = Clean(chunk.Text);
        var paragraphs = SplitParagraphs(text);
        var codeBlocks = ExtractCodeBlocks(text);
        var drafts = new List<StudyItemDraft>();

        drafts.Add(new StudyItemDraft(
            StudyItemKind.Concept,
            $"Explain the main idea of \"{chunk.Heading}\" in your own words.",
            FirstStrongParagraph(paragraphs),
            $"This checks whether you can summarize pages {chunk.StartPage}-{chunk.EndPage} from {chunk.SourceMaterial.Title}.",
            FirstStrongParagraph(paragraphs)));

        foreach (var definition in FindDefinitionParagraphs(paragraphs).Take(2))
        {
            drafts.Add(new StudyItemDraft(
                StudyItemKind.Flashcard,
                BuildDefinitionPrompt(definition),
                definition,
                "Use the source wording to anchor the definition, then rewrite it from memory.",
                definition));
        }

        foreach (var paragraph in paragraphs.Where(IsExplanatoryParagraph).Take(2))
        {
            drafts.Add(new StudyItemDraft(
                StudyItemKind.ShortAnswer,
                BuildWhyHowPrompt(paragraph),
                paragraph,
                "A good answer should identify the cause/effect relationship described in the source.",
                paragraph));
        }

        foreach (var code in codeBlocks.Take(2))
        {
            drafts.Add(new StudyItemDraft(
                StudyItemKind.CodeReading,
                "Read the code snippet. What will it do, return, or print, and why?",
                InferCodeAnswer(code),
                "Focus on control flow, scope, state changes, and the exact output or behavior implied by the snippet.",
                code));
        }

        if (codeBlocks.Count > 0)
        {
            var code = codeBlocks[0];
            drafts.Add(new StudyItemDraft(
                StudyItemKind.CodingExercise,
                BuildCodingExercisePrompt(chunk.Heading, code),
                "Write a working implementation that preserves the behavior demonstrated by the source snippet. Run it and explain the result.",
                "This turns passive code reading into a small implementation exercise tied to the book section.",
                code));
        }

        return drafts
            .Where(IsUsable)
            .DistinctBy(draft => (draft.Kind, NormalizeForKey(draft.Prompt)))
            .Take(8)
            .ToList();
    }

    public async Task<IReadOnlyCollection<StudyItemDraft>> GenerateForTopicAsync(
        string topicSlug,
        CancellationToken cancellationToken)
    {
        var topic = await db.Topics
            .AsNoTracking()
            .Where(topic => topic.Slug == topicSlug)
            .Select(topic => new
            {
                topic.Id,
                topic.Title,
                topic.Summary,
                ModuleTitle = topic.Module.Title,
                TaskTypes = topic.TaskTypes.Select(taskType => taskType.TaskType).ToList()
            })
            .SingleAsync(cancellationToken);

        var note = await db.TopicNotes
            .AsNoTracking()
            .Where(note => note.TopicId == topic.Id)
            .Select(note => note.Content)
            .SingleOrDefaultAsync(cancellationToken);

        var resourceRows = await db.TopicResourceLinks
            .AsNoTracking()
            .Where(link => link.TopicId == topic.Id)
            .Include(link => link.LearningResource)
            .Include(link => link.VideoCandidate)
            .OrderByDescending(link => link.Priority)
            .Take(4)
            .ToListAsync(cancellationToken);

        var videoContext = resourceRows
            .Select(link => link.LearningResource is not null
                ? $"{link.LearningResource.Title} by {link.LearningResource.Creator}: {link.LearningResource.Summary}"
                : $"{link.VideoCandidate!.Title} by {link.VideoCandidate.ChannelName}: {link.VideoCandidate.Summary}")
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();

        var searchTerms = TopicSearchTerms(topic.Title, topic.Summary);
        var sourceParagraph = await FindSourceParagraphAsync(searchTerms, cancellationToken)
            ?? topic.Summary
            ?? $"Study the topic {topic.Title} in the {topic.Title} module and connect it to implementation practice.";
        var focus = ContentFocus.For(topic.Title, topic.Summary, topic.ModuleTitle);

        var context = string.Join(
            Environment.NewLine,
            new[]
            {
                $"Topic: {topic.Title}",
                $"Summary: {topic.Summary}",
                $"Module: {topic.ModuleTitle}",
                $"Task types: {string.Join(", ", topic.TaskTypes)}",
                string.IsNullOrWhiteSpace(note) ? null : $"Notes: {note}",
                videoContext.Count == 0 ? null : $"Videos: {string.Join(" | ", videoContext)}",
                $"Source context: {sourceParagraph}"
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var excerpt = context.Length > 1_500 ? context[..1_500] : context;
        var drafts = new List<StudyItemDraft>
        {
            new(
                StudyItemKind.Concept,
                $"Explain {topic.Title} as if you were reviewing a production codebase.",
                $"A strong answer should define the topic, name the production boundary it affects, explain the relevant {focus.LibraryName} APIs, and show how the implementation choice changes correctness, maintainability, or operability. Use this source context: {sourceParagraph}",
                $"This checks whether you can turn {focus.LibraryName} material into production engineering language instead of repeating a definition.",
                excerpt),
            new(
                StudyItemKind.ShortAnswer,
                $"What can go wrong when {topic.Title} is implemented casually, and how would you detect it?",
                $"Name at least two failure modes, the observable symptoms, and the feedback mechanism you would use: {focus.FeedbackMechanisms}. Include at least one detail from the underlying API/runtime behavior.",
                "The answer should connect the topic to concrete failure modes and verification, not generic best-practice wording.",
                excerpt),
            new(
                StudyItemKind.ShortAnswer,
                $"Compare two implementation approaches for {topic.Title}. Which one would you choose for this platform and why?",
                $"A good answer states the tradeoff, names the relevant APIs ({focus.ApiExamples}), gives the chosen approach, and explains why it fits a single-user local-first study platform that is intended to go to production.",
                "This forces design judgment and prevents shallow memorization.",
                excerpt),
            new(
                StudyItemKind.Flashcard,
                $"Name the core APIs you must know for {topic.Title}.",
                $"Know what these are for, what they return, and when not to use them: {focus.ApiExamples}. Tie each API to one real scenario in this app.",
                "This builds the standard-library vocabulary needed before solving production exercises.",
                excerpt),
            new(
                StudyItemKind.ShortAnswer,
                $"Use the books/videos to triangulate {topic.Title}: what does the source explain, what does the video add, and what would you verify in code?",
                $"A strong answer cites one source idea, one video/resource idea if linked, and one concrete check. The code check should prove behavior around {focus.FailureModes}.",
                "This prevents shallow consumption by forcing reading, watching, and coding into the same study loop.",
                excerpt),
            new(
                StudyItemKind.CodeReading,
                $"Read a handler, component, or service related to {topic.Title}. What behavior should you verify before changing it?",
                "Identify the inputs, state changes, side effects, persistence boundary, and the user-visible result. Then name the smallest regression check that would catch a bad change.",
                "Use this as a code-review prompt when studying real code for the topic.",
                excerpt),
            new(
                StudyItemKind.CodingExercise,
                $"Implement a focused example that demonstrates {topic.Title}.",
                $"Build a small, runnable implementation with clear inputs and outputs. Include one success path, one failure path, and notes on package/runtime requirements. The implementation should exercise: {focus.ApiExamples}.",
                "This is the bridge from reading/watching into the Monaco exercise surface.",
                excerpt)
        };

        foreach (var video in videoContext.Take(2))
        {
            drafts.Add(new StudyItemDraft(
                StudyItemKind.ShortAnswer,
                $"After watching the linked video, what is the most important production detail for {topic.Title}?",
                $"Use the video context to identify one advanced detail, explain why it matters, and write down how you would apply it in this app. Video context: {video}",
                "Video-linked questions should extract applied engineering decisions, not just restate the title.",
                video));
        }

        return drafts
            .Where(IsUsable)
            .DistinctBy(draft => (draft.Kind, NormalizeForKey(draft.Prompt)))
            .Take(8)
            .ToList();
    }

    private static bool IsUsable(StudyItemDraft draft)
    {
        return draft.Prompt.Length >= 20
            && draft.ExpectedAnswer.Length >= 40
            && draft.SourceExcerpt.Length >= 40;
    }

    private static string Clean(string text)
    {
        return text.Replace("\u0008", string.Empty).Trim();
    }

    private static IReadOnlyCollection<string> SplitParagraphs(string text)
    {
        return text.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(paragraph => RepeatedWhitespaceRegex().Replace(paragraph, " ").Trim())
            .Where(paragraph => paragraph.Length is >= 80 and <= 1200)
            .Where(paragraph => !PageMarkerRegex().IsMatch(paragraph))
            .ToList();
    }

    private static string FirstStrongParagraph(IReadOnlyCollection<string> paragraphs)
    {
        return paragraphs.FirstOrDefault(IsExplanatoryParagraph)
            ?? paragraphs.FirstOrDefault()
            ?? "Review the selected source chunk and identify the most important idea before answering.";
    }

    private static IEnumerable<string> FindDefinitionParagraphs(IEnumerable<string> paragraphs)
    {
        return paragraphs.Where(paragraph =>
            DefinitionRegex().IsMatch(paragraph)
            || paragraph.Contains(" is ", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains(" are ", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsExplanatoryParagraph(string paragraph)
    {
        return paragraph.Contains("because", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains("therefore", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains("this means", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains("this is", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains("when ", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains("if ", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildDefinitionPrompt(string paragraph)
    {
        var subject = paragraph.Split(['.', ':', ';'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(subject) || subject.Length > 140)
        {
            return "Define the concept described in the source excerpt.";
        }

        return $"Define or explain this concept: {subject}";
    }

    private static string BuildWhyHowPrompt(string paragraph)
    {
        var trimmed = paragraph.Length > 180 ? $"{paragraph[..180].Trim()}..." : paragraph;
        return $"Why does this matter, and how would you apply it? Source claim: {trimmed}";
    }

    private static IReadOnlyList<string> ExtractCodeBlocks(string text)
    {
        var lines = text.Split('\n');
        var blocks = new List<string>();
        var buffer = new List<string>();

        foreach (var line in lines)
        {
            if (LooksLikeCode(line))
            {
                buffer.Add(line.TrimEnd());
                continue;
            }

            if (buffer.Count >= 3)
            {
                blocks.Add(string.Join(Environment.NewLine, buffer).Trim());
            }

            buffer.Clear();
        }

        if (buffer.Count >= 3)
        {
            blocks.Add(string.Join(Environment.NewLine, buffer).Trim());
        }

        return blocks
            .Where(block => block.Count(char.IsLetterOrDigit) >= 20)
            .Distinct()
            .ToList();
    }

    private static bool LooksLikeCode(string line)
    {
        var value = line.Trim();
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Length > 180 && !value.Contains('{') && !value.Contains(';')) return false;

        return value.StartsWith("function ", StringComparison.Ordinal)
            || value.StartsWith("public ", StringComparison.Ordinal)
            || value.StartsWith("private ", StringComparison.Ordinal)
            || value.StartsWith("class ", StringComparison.Ordinal)
            || value.StartsWith("using ", StringComparison.Ordinal)
            || value.StartsWith("const ", StringComparison.Ordinal)
            || value.StartsWith("let ", StringComparison.Ordinal)
            || value.StartsWith("var ", StringComparison.Ordinal)
            || value.StartsWith("if ", StringComparison.Ordinal)
            || value.StartsWith("for ", StringComparison.Ordinal)
            || value.StartsWith("while ", StringComparison.Ordinal)
            || value.StartsWith("console.", StringComparison.Ordinal)
            || value is "}" or "{" or ");"
            || value.EndsWith(';')
            || value.EndsWith('{')
            || value.EndsWith('}');
    }

    private static string InferCodeAnswer(string code)
    {
        if (code.Contains("console.log", StringComparison.Ordinal))
        {
            return "Trace each console.log call in order. Pay attention to variable scope, assignments, and whether any line throws before later statements run.";
        }

        if (code.Contains("return ", StringComparison.Ordinal))
        {
            return "Trace the function inputs and return path. The answer should state the returned value and why that branch is reached.";
        }

        return "Explain the snippet line by line, naming the state changes, scope rules, side effects, and final observable behavior.";
    }

    private static string BuildCodingExercisePrompt(string heading, string code)
    {
        var firstLine = code.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        return string.IsNullOrWhiteSpace(firstLine)
            ? $"Create a small runnable example that demonstrates the idea from \"{heading}\"."
            : $"Recreate and modify this example from \"{heading}\" so it still demonstrates the same rule: {firstLine}";
    }

    private static string NormalizeForKey(string value)
    {
        return RepeatedWhitespaceRegex().Replace(value.Trim().ToLowerInvariant(), " ");
    }

    private async Task<string?> FindSourceParagraphAsync(
        IReadOnlyCollection<string> searchTerms,
        CancellationToken cancellationToken)
    {
        if (searchTerms.Count == 0)
        {
            return null;
        }

        var chunks = await db.SourceDocumentChunks
            .AsNoTracking()
            .Where(chunk => chunk.Text.Length > 500)
            .OrderByDescending(chunk => chunk.CharacterCount)
            .Take(250)
            .Select(chunk => chunk.Text)
            .ToListAsync(cancellationToken);

        return chunks
            .SelectMany(text => SplitParagraphs(Clean(text)))
            .Where(paragraph => searchTerms.Any(term => paragraph.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(paragraph => searchTerms.Count(term => paragraph.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ThenBy(paragraph => paragraph.Length)
            .FirstOrDefault();
    }

    private static IReadOnlyCollection<string> TopicSearchTerms(string title, string? summary)
    {
        return $"{title} {summary}"
            .Split([' ', '-', '/', '.', ',', ':', ';', '(', ')'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(term => term.Length >= 5)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Concat(ExtraSearchTerms(title, summary))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(14)
            .ToList();
    }

    private static IReadOnlyCollection<string> ExtraSearchTerms(string title, string? summary)
    {
        var haystack = $"{title} {summary}".ToLowerInvariant();
        if (haystack.Contains("bcl") || haystack.Contains(".net") || haystack.Contains("span") || haystack.Contains("linq"))
        {
            return ["System", "Task", "Stream", "Span", "Memory", "LINQ", "DateTime", "JsonSerializer"];
        }

        if (haystack.Contains("node") || haystack.Contains("javascript") || haystack.Contains("promise") || haystack.Contains("stream"))
        {
            return ["Promise", "Array", "Map", "Set", "Buffer", "stream", "fs", "crypto", "AbortController"];
        }

        if (haystack.Contains("react") || haystack.Contains("typescript"))
        {
            return ["React", "TypeScript", "component", "hook", "props", "state", "generic", "union"];
        }

        if (haystack.Contains("asp.net") || haystack.Contains("api") || haystack.Contains("middleware"))
        {
            return ["middleware", "endpoint", "ProblemDetails", "OpenAPI", "HttpContext", "validation"];
        }

        return [];
    }

    private sealed record ContentFocus(
        string LibraryName,
        string ApiExamples,
        string FailureModes,
        string FeedbackMechanisms)
    {
        public static ContentFocus For(string title, string? summary, string moduleTitle)
        {
            var haystack = $"{title} {summary} {moduleTitle}".ToLowerInvariant();

            if (haystack.Contains("node") || haystack.Contains("javascript") || haystack.Contains("promise"))
            {
                if (haystack.Contains("stream") || haystack.Contains("buffer"))
                {
                    return new ContentFocus(
                        "Node stream and Buffer APIs",
                        "Readable, Writable, Transform, stream.pipeline, finished, Buffer.from, Buffer.byteLength, TextEncoder, TextDecoder",
                        "backpressure bugs, partial reads, encoding corruption, memory spikes, unhandled stream errors, missing abort behavior",
                        "large-file integration tests, error-path tests, memory checks, backpressure tests, and runtime logs");
                }

                if (haystack.Contains("crypto"))
                {
                    return new ContentFocus(
                        "Node crypto APIs",
                        "crypto.randomUUID, randomBytes, createHash, createHmac, timingSafeEqual, webcrypto.subtle",
                        "predictable tokens, unsafe comparisons, encoding mistakes, weak hashing, and leaking secrets into logs",
                        "unit tests with known vectors, security review, token-length checks, and failure-path tests");
                }

                return new ContentFocus(
                    "JavaScript/Node standard library",
                    "Array, Map, Set, Promise, AbortController, URL, fs/promises, stream.pipeline, Buffer, crypto, performance",
                    "event-loop ordering, unhandled rejections, encoding bugs, backpressure, path portability, timeout/cancellation leaks",
                    "unit tests, integration tests, runtime assertions, performance measurements, logs, and error-path tests");
            }

            if (haystack.Contains("react") || haystack.Contains("typescript"))
            {
                return new ContentFocus(
                    "React and TypeScript core APIs",
                    "discriminated unions, generics, mapped types, React state, effects, controlled inputs, TanStack Query cache keys",
                    "stale server state, invalid runtime data, over-broad types, inaccessible controls, unnecessary re-renders",
                    "component tests, type-level checks, API mocks, accessibility checks, and browser smoke tests");
            }

            if (haystack.Contains("asp.net") || haystack.Contains("minimal api") || haystack.Contains("middleware") || haystack.Contains("openapi"))
            {
                return new ContentFocus(
                    "ASP.NET Core APIs",
                    "WebApplication, route groups, TypedResults, ProblemDetails, endpoint filters, HttpContext, middleware, OpenAPI",
                    "unstable contracts, inconsistent errors, missed cancellation, wrong middleware ordering, hidden validation behavior",
                    "integration tests, OpenAPI review, logs, traces, ProblemDetails assertions, and handler-level tests");
            }

            if (haystack.Contains("ef core") || haystack.Contains("sqlite") || haystack.Contains("persistence"))
            {
                return new ContentFocus(
                    "EF Core and SQLite APIs",
                    "DbContext, DbSet, IEntityTypeConfiguration, migrations, Select projections, AsNoTracking, concurrency tokens",
                    "N+1 queries, over-fetching, migration data loss, tracking bugs, concurrency conflicts, SQLite constraint surprises",
                    "integration tests, SQL log review, migration review, query-shape inspection, and conflict-path tests");
            }

            if (haystack.Contains("json"))
            {
                return new ContentFocus(
                    "System.Text.Json APIs",
                    "JsonSerializer, JsonSerializerOptions, JsonNamingPolicy, JsonConverter<T>, JsonRequiredAttribute, JsonTypeInfo, source generation",
                    "contract drift, missing required data, casing mismatches, lossy converters, versioning breaks, and unsafe polymorphism",
                    "serializer round-trip tests, golden JSON fixtures, API contract tests, converter edge cases, and OpenAPI comparison");
            }

            if (haystack.Contains("span") || haystack.Contains("memory") || haystack.Contains("allocation"))
            {
                return new ContentFocus(
                    ".NET memory APIs",
                    "Span<T>, ReadOnlySpan<T>, Memory<T>, ArrayPool<T>, MemoryMarshal, stackalloc, BenchmarkDotNet",
                    "hidden allocations, buffer lifetime bugs, slicing mistakes, pool misuse, and premature low-level complexity",
                    "benchmarks, allocation checks, edge-case unit tests, profiling, and code review");
            }

            if (haystack.Contains("time") || haystack.Contains("datetime") || haystack.Contains("dateonly"))
            {
                return new ContentFocus(
                    ".NET time APIs",
                    "DateTimeOffset, DateOnly, TimeOnly, TimeZoneInfo, TimeProvider, PeriodicTimer, Stopwatch",
                    "local/UTC confusion, non-deterministic tests, DST bugs, clock drift, and incorrect date-only modeling",
                    "clock-injected tests, timezone fixtures, boundary-date tests, and scheduling integration tests");
            }

            if (haystack.Contains("task") || haystack.Contains("async") || haystack.Contains("cancellation"))
            {
                return new ContentFocus(
                    ".NET async APIs",
                    "Task, ValueTask, CancellationToken, Task.WhenAll, Task.WaitAsync, PeriodicTimer, IAsyncEnumerable<T>",
                    "deadlocks, swallowed cancellation, unobserved exceptions, runaway concurrency, timeout leaks, and sync-over-async blocking",
                    "cancellation tests, timeout tests, exception-path tests, concurrency limits, and structured logs");
            }

            if (haystack.Contains("linq"))
            {
                return new ContentFocus(
                    "LINQ APIs",
                    "IEnumerable<T>, IQueryable<T>, Select, Where, GroupBy, Join, ToList, Any, FirstOrDefault, DistinctBy",
                    "deferred execution surprises, repeated enumeration, client-side filtering, null handling, and accidental expensive queries",
                    "unit tests with side-effecting enumerables, SQL/query inspection, benchmark checks, and code review");
            }

            return new ContentFocus(
                ".NET BCL",
                "collections, LINQ, Task, CancellationToken, Stream, Path, TimeProvider, System.Text.Json, ILogger, Activity",
                "allocation pressure, deferred execution surprises, culture bugs, clock instability, stream disposal, cancellation leaks",
                "unit tests, integration tests, benchmarks, logs, traces, and edge-case fixtures");
        }
    }

    [GeneratedRegex(@"\b(is|are|means|refers to|is called|are called)\b", RegexOptions.IgnoreCase)]
    private static partial Regex DefinitionRegex();

    [GeneratedRegex(@"\[\s*\d+\s*\]|^Chapter\s+\d+", RegexOptions.IgnoreCase)]
    private static partial Regex PageMarkerRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex RepeatedWhitespaceRegex();
}
