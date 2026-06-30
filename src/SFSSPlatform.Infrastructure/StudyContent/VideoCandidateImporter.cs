using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed partial class VideoCandidateImporter(
    StudyPlatformDbContext db,
    IHostEnvironment environment,
    IOptions<VideoCandidateImportOptions> options,
    TimeProvider timeProvider)
{
    private static readonly string[] Header =
    [
        "title",
        "channelName",
        "channelUrl",
        "videoUrl",
        "tags",
        "difficulty",
        "summary",
        "notes"
    ];

    public async Task<VideoCandidateImportResult> ImportLocalAsync(CancellationToken cancellationToken)
    {
        var sourceFile = ResolveSourceFile();
        if (!File.Exists(sourceFile))
        {
            throw new FileNotFoundException($"Video candidate file does not exist: {sourceFile}", sourceFile);
        }

        var rows = await ReadRowsAsync(sourceFile, cancellationToken);
        var now = timeProvider.GetUtcNow();
        var created = 0;
        var updated = 0;
        var skipped = 0;

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var externalId = TryExtractYouTubeId(row.VideoUrl);
            if (string.IsNullOrWhiteSpace(row.Title)
                || string.IsNullOrWhiteSpace(row.ChannelName)
                || string.IsNullOrWhiteSpace(row.ChannelUrl)
                || externalId is null)
            {
                skipped++;
                continue;
            }

            var url = $"https://www.youtube.com/watch?v={externalId}";
            var embedUrl = $"https://www.youtube-nocookie.com/embed/{externalId}";
            var existing = await db.VideoCandidates
                .SingleOrDefaultAsync(candidate => candidate.ExternalId == externalId, cancellationToken);

            if (existing is null)
            {
                db.VideoCandidates.Add(new VideoCandidate(
                    externalId,
                    row.Title,
                    row.ChannelName,
                    NormalizeUrl(row.ChannelUrl),
                    url,
                    embedUrl,
                    row.Summary,
                    NormalizeTags(row.Tags),
                    row.Difficulty,
                    row.Notes,
                    now));
                created++;
                continue;
            }

            existing.SyncMetadata(
                row.Title,
                row.ChannelName,
                NormalizeUrl(row.ChannelUrl),
                url,
                embedUrl,
                row.Summary,
                NormalizeTags(row.Tags),
                row.Difficulty,
                row.Notes,
                now);
            updated++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new VideoCandidateImportResult(sourceFile, rows.Count, created, updated, skipped);
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
                var candidate = Path.Combine(current.FullName, folderName, "youtube", "video-candidates.csv");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            current = current.Parent;
        }

        return Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "local-source", "youtube", "video-candidates.csv"));
    }

    private static async Task<IReadOnlyCollection<VideoCandidateRow>> ReadRowsAsync(
        string sourceFile,
        CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(sourceFile, cancellationToken);
        if (lines.Length == 0)
        {
            return [];
        }

        var startIndex = IsHeader(ParseCsvLine(lines[0])) ? 1 : 0;
        var rows = new List<VideoCandidateRow>();

        for (var index = startIndex; index < lines.Length; index++)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            var cells = ParseCsvLine(line);
            if (cells.Count < 4)
            {
                rows.Add(new VideoCandidateRow(
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    VideoCandidateDifficulty.Intermediate,
                    string.Empty,
                    string.Empty));
                continue;
            }

            rows.Add(new VideoCandidateRow(
                cells[0],
                cells[1],
                cells[2],
                cells[3],
                cells.Count > 4 ? cells[4] : string.Empty,
                cells.Count > 5 ? ParseDifficulty(cells[5]) : VideoCandidateDifficulty.Intermediate,
                cells.Count > 6 ? cells[6] : string.Empty,
                cells.Count > 7 ? cells[7] : string.Empty));
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

    private static VideoCandidateDifficulty ParseDifficulty(string value)
    {
        return Enum.TryParse<VideoCandidateDifficulty>(value, ignoreCase: true, out var difficulty)
            ? difficulty
            : VideoCandidateDifficulty.Intermediate;
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

    private static string? TryExtractYouTubeId(string url)
    {
        var match = YouTubeIdRegex().Match(url.Trim());
        return match.Success ? match.Groups["id"].Value : null;
    }

    [GeneratedRegex(@"(?:youtu\.be/|youtube\.com/(?:watch\?v=|embed/|shorts/))(?<id>[A-Za-z0-9_-]{11})", RegexOptions.IgnoreCase)]
    private static partial Regex YouTubeIdRegex();
}

public sealed record VideoCandidateRow(
    string Title,
    string ChannelName,
    string ChannelUrl,
    string VideoUrl,
    string Tags,
    VideoCandidateDifficulty Difficulty,
    string Summary,
    string Notes);
