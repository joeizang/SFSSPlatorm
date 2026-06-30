using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed class TrustedYouTubeChannelImporter(
    StudyPlatformDbContext db,
    IHostEnvironment environment,
    IOptions<TrustedYouTubeChannelImportOptions> options,
    TimeProvider timeProvider)
{
    private static readonly string[] Header =
    [
        "channelName",
        "channelUrl",
        "tags",
        "priority",
        "notes"
    ];

    public async Task<TrustedYouTubeChannelImportResult> ImportLocalAsync(CancellationToken cancellationToken)
    {
        var sourceFile = ResolveSourceFile();
        if (!File.Exists(sourceFile))
        {
            throw new FileNotFoundException($"Trusted YouTube channel file does not exist: {sourceFile}", sourceFile);
        }

        var rows = await ReadRowsAsync(sourceFile, cancellationToken);
        var created = 0;
        var updated = 0;
        var skipped = 0;
        var now = timeProvider.GetUtcNow();

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(row.Name) || string.IsNullOrWhiteSpace(row.Url))
            {
                skipped++;
                continue;
            }

            var url = NormalizeUrl(row.Url);
            var existing = await db.TrustedYouTubeChannels
                .SingleOrDefaultAsync(channel => channel.Url == url, cancellationToken);

            if (existing is null)
            {
                db.TrustedYouTubeChannels.Add(new TrustedYouTubeChannel(
                    row.Name,
                    url,
                    NormalizeTags(row.Tags),
                    row.Priority,
                    row.Notes,
                    now));
                created++;
                continue;
            }

            existing.SyncMetadata(
                row.Name,
                NormalizeTags(row.Tags),
                row.Priority,
                row.Notes,
                now);
            updated++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new TrustedYouTubeChannelImportResult(sourceFile, rows.Count, created, updated, skipped);
    }

    public string ResolveSourceFile()
    {
        if (!string.IsNullOrWhiteSpace(options.Value.SourceFile))
        {
            return Path.GetFullPath(options.Value.SourceFile);
        }

        var current = new DirectoryInfo(environment.ContentRootPath);
        while (current is not null)
        {
            foreach (var folderName in new[] { "local-source", "local-sources" })
            {
                var candidate = Path.Combine(current.FullName, folderName, "youtube", "channels.csv");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            current = current.Parent;
        }

        return Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "local-source", "youtube", "channels.csv"));
    }

    private static async Task<IReadOnlyCollection<TrustedYouTubeChannelRow>> ReadRowsAsync(
        string sourceFile,
        CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(sourceFile, cancellationToken);
        if (lines.Length == 0)
        {
            return [];
        }

        var startIndex = IsHeader(ParseCsvLine(lines[0])) ? 1 : 0;
        var rows = new List<TrustedYouTubeChannelRow>();

        for (var index = startIndex; index < lines.Length; index++)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            var cells = ParseCsvLine(line);
            if (cells.Count < 2)
            {
                rows.Add(new TrustedYouTubeChannelRow(string.Empty, string.Empty, string.Empty, TrustedChannelPriority.Medium, string.Empty));
                continue;
            }

            rows.Add(new TrustedYouTubeChannelRow(
                cells[0],
                cells[1],
                cells.Count > 2 ? cells[2] : string.Empty,
                cells.Count > 3 ? ParsePriority(cells[3]) : TrustedChannelPriority.Medium,
                cells.Count > 4 ? cells[4] : string.Empty));
        }

        return rows;
    }

    private static bool IsHeader(IReadOnlyList<string> cells)
    {
        return cells.Count >= Header.Length
            && Header.Zip(cells, (expected, actual) => string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                .All(match => match);
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var cells = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];
            if (character == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (character == ',' && !inQuotes)
            {
                cells.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        cells.Add(current.ToString().Trim());
        return cells;
    }

    private static TrustedChannelPriority ParsePriority(string value)
    {
        return Enum.TryParse<TrustedChannelPriority>(value, ignoreCase: true, out var priority)
            ? priority
            : TrustedChannelPriority.Medium;
    }

    private static string NormalizeTags(string tags)
    {
        return string.Join(
            ',',
            tags.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(tag => tag.ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase));
    }

    private static string NormalizeUrl(string url)
    {
        return url.Trim().TrimEnd('/');
    }
}

public sealed record TrustedYouTubeChannelRow(
    string Name,
    string Url,
    string Tags,
    TrustedChannelPriority Priority,
    string Notes);
