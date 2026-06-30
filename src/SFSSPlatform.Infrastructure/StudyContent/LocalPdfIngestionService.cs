using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed partial class LocalPdfIngestionService(
    StudyPlatformDbContext db,
    IHostEnvironment environment,
    IOptions<LocalPdfIngestionOptions> options,
    TimeProvider timeProvider)
{
    public async Task<LocalPdfIngestionResult> IngestFocusedSourcesAsync(
        bool force,
        CancellationToken cancellationToken)
    {
        var sourceFolder = ResolveSourceFolder();
        if (!Directory.Exists(sourceFolder))
        {
            throw new DirectoryNotFoundException($"Local source folder does not exist: {sourceFolder}");
        }

        var pdfs = Directory.EnumerateFiles(sourceFolder, "*.pdf", SearchOption.TopDirectoryOnly)
            .Where(path => FocusedSourceCatalog.FileNames.Contains(Path.GetFileName(path)))
            .OrderBy(Path.GetFileName)
            .ToList();

        var ingested = 0;
        var skipped = 0;
        var failed = 0;
        var chunksCreated = 0;

        foreach (var pdfPath in pdfs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(pdfPath);
            var stableKey = StableKey(fileName);
            var existing = await db.SourceMaterials
                .Include(source => source.Chunks)
                .SingleOrDefaultAsync(source => source.StableKey == stableKey, cancellationToken);

            if (existing?.ExtractionStatus == ExtractionStatus.Completed && !force)
            {
                skipped++;
                continue;
            }

            var metadata = await ReadMetadataAsync(pdfPath, cancellationToken);
            var source = existing ?? new SourceMaterial(
                stableKey,
                metadata.Title,
                metadata.Author,
                fileName,
                RelativePath(pdfPath),
                FocusedSourceCatalog.GetAccess(fileName),
                new FileInfo(pdfPath).Length);

            source.SyncMetadata(
                metadata.Title,
                metadata.Author,
                fileName,
                RelativePath(pdfPath),
                FocusedSourceCatalog.GetAccess(fileName),
                new FileInfo(pdfPath).Length,
                metadata.PageCount);

            if (existing is null)
            {
                db.SourceMaterials.Add(source);
                await db.SaveChangesAsync(cancellationToken);
            }

            if (force)
            {
                await db.SourceDocumentChunks
                    .Where(chunk => chunk.SourceMaterialId == source.Id)
                    .ExecuteDeleteAsync(cancellationToken);
            }

            try
            {
                var chunks = await ExtractChunksAsync(pdfPath, source.Id, metadata.PageCount, cancellationToken);
                db.SourceDocumentChunks.AddRange(chunks);
                source.MarkCompleted(metadata.PageCount, timeProvider.GetUtcNow());
                chunksCreated += chunks.Count;
                ingested++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                source.MarkFailed(ex.Message, timeProvider.GetUtcNow());
                failed++;
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        return new LocalPdfIngestionResult(pdfs.Count, ingested, skipped, failed, chunksCreated);
    }

    private async Task<List<SourceDocumentChunk>> ExtractChunksAsync(
        string pdfPath,
        int sourceMaterialId,
        int pageCount,
        CancellationToken cancellationToken)
    {
        var text = await RunToolAsync("pdftotext", ["-layout", pdfPath, "-"], cancellationToken);
        var pages = text.Split('\f')
            .Select(NormalizeText)
            .ToArray();

        var chunks = new List<SourceDocumentChunk>();
        var pagesPerChunk = Math.Max(1, options.Value.PagesPerChunk);
        var order = 1;

        for (var start = 1; start <= pageCount; start += pagesPerChunk)
        {
            var end = Math.Min(pageCount, start + pagesPerChunk - 1);
            var chunkText = string.Join(
                Environment.NewLine + Environment.NewLine,
                Enumerable.Range(start, end - start + 1)
                    .Select(page => page - 1 < pages.Length ? pages[page - 1] : string.Empty)
                    .Where(pageText => !string.IsNullOrWhiteSpace(pageText)));

            if (string.IsNullOrWhiteSpace(chunkText) || CountMeaningfulCharacters(chunkText) < 120)
            {
                continue;
            }

            var heading = GuessHeading(chunkText, start, end);
            chunks.Add(new SourceDocumentChunk(sourceMaterialId, order, start, end, heading, chunkText));
            order++;
        }

        if (chunks.Count == 0)
        {
            throw new InvalidOperationException("No readable text chunks were extracted.");
        }

        return chunks;
    }

    private async Task<PdfMetadata> ReadMetadataAsync(string pdfPath, CancellationToken cancellationToken)
    {
        var output = await RunToolAsync("pdfinfo", [pdfPath], cancellationToken);
        var values = output.Split(Environment.NewLine)
            .Select(line => line.Split(':', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);

        var pageCount = values.TryGetValue("Pages", out var pages) && int.TryParse(pages, out var parsedPages)
            ? parsedPages
            : throw new InvalidOperationException($"Could not read page count for {Path.GetFileName(pdfPath)}.");

        var title = values.TryGetValue("Title", out var pdfTitle) && !string.IsNullOrWhiteSpace(pdfTitle)
            ? pdfTitle
            : TitleFromFileName(Path.GetFileNameWithoutExtension(pdfPath));

        var author = values.TryGetValue("Author", out var pdfAuthor) && !string.IsNullOrWhiteSpace(pdfAuthor)
            ? pdfAuthor
            : null;

        return new PdfMetadata(title, author, pageCount);
    }

    private string ResolveSourceFolder()
    {
        if (!string.IsNullOrWhiteSpace(options.Value.SourceFolder))
        {
            return Path.GetFullPath(options.Value.SourceFolder);
        }

        var current = new DirectoryInfo(environment.ContentRootPath);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "local-sources", "pdfs");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "local-sources", "pdfs"));
    }

    private static async Task<string> RunToolAsync(
        string executable,
        IReadOnlyCollection<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {executable}.");

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{executable} failed: {error}");
        }

        return output;
    }

    private static string RelativePath(string path)
    {
        return Path.GetRelativePath(Directory.GetCurrentDirectory(), path);
    }

    private static string StableKey(string fileName)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(fileName.ToLowerInvariant()));
        return Convert.ToHexString(bytes)[..24].ToLowerInvariant();
    }

    private static string NormalizeText(string text)
    {
        var normalizedLines = text.Replace("\r", string.Empty)
            .Split('\n')
            .Select(line => RepeatedHorizontalWhitespaceRegex().Replace(line, " ").Trim())
            .ToArray();

        var normalizedText = string.Join('\n', normalizedLines);
        return RepeatedBlankLineRegex().Replace(normalizedText, $"{Environment.NewLine}{Environment.NewLine}").Trim();
    }

    private static int CountMeaningfulCharacters(string text)
    {
        return text.Count(char.IsLetterOrDigit);
    }

    private static string GuessHeading(string text, int startPage, int endPage)
    {
        var line = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim())
            .FirstOrDefault(value => value.Length is > 8 and < 120);

        return string.IsNullOrWhiteSpace(line) ? $"Pages {startPage}-{endPage}" : line;
    }

    private static string TitleFromFileName(string fileName)
    {
        var withoutIsbn = IsbnPrefixRegex().Replace(fileName, string.Empty);
        var cleaned = withoutIsbn
            .Replace('_', ' ')
            .Replace('-', ' ')
            .Replace("  ", " ")
            .Trim();

        return string.IsNullOrWhiteSpace(cleaned) ? fileName : cleaned;
    }

    [GeneratedRegex(@"^\d{13}-")]
    private static partial Regex IsbnPrefixRegex();

    [GeneratedRegex(@"[ \t]{2,}")]
    private static partial Regex RepeatedHorizontalWhitespaceRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex RepeatedBlankLineRegex();

    private sealed record PdfMetadata(string Title, string? Author, int PageCount);
}
