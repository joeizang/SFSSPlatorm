namespace SFSSPlatform.Domain.StudyContent;

public sealed class SourceMaterial
{
    private readonly List<SourceDocumentChunk> _chunks = [];

    private SourceMaterial()
    {
    }

    public SourceMaterial(
        string stableKey,
        string title,
        string? author,
        string fileName,
        string relativePath,
        SourceAccess access,
        long fileSizeBytes)
    {
        StableKey = Required(stableKey);
        Title = Required(title);
        Author = string.IsNullOrWhiteSpace(author) ? null : author.Trim();
        FileName = Required(fileName);
        RelativePath = Required(relativePath);
        Access = access;
        FileSizeBytes = fileSizeBytes;
        ExtractionStatus = ExtractionStatus.NotStarted;
    }

    public int Id { get; private set; }

    public string StableKey { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Author { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string RelativePath { get; private set; } = string.Empty;

    public SourceAccess Access { get; private set; }

    public long FileSizeBytes { get; private set; }

    public int? PageCount { get; private set; }

    public ExtractionStatus ExtractionStatus { get; private set; }

    public string? ExtractionError { get; private set; }

    public DateTimeOffset? ExtractedAt { get; private set; }

    public IReadOnlyCollection<SourceDocumentChunk> Chunks => _chunks;

    public void SyncMetadata(
        string title,
        string? author,
        string fileName,
        string relativePath,
        SourceAccess access,
        long fileSizeBytes,
        int? pageCount)
    {
        Title = Required(title);
        Author = string.IsNullOrWhiteSpace(author) ? null : author.Trim();
        FileName = Required(fileName);
        RelativePath = Required(relativePath);
        Access = access;
        FileSizeBytes = fileSizeBytes;
        PageCount = pageCount;
    }

    public void MarkCompleted(int pageCount, DateTimeOffset extractedAt)
    {
        PageCount = pageCount;
        ExtractionStatus = ExtractionStatus.Completed;
        ExtractionError = null;
        ExtractedAt = extractedAt;
    }

    public void MarkFailed(string error, DateTimeOffset attemptedAt)
    {
        ExtractionStatus = ExtractionStatus.Failed;
        ExtractionError = string.IsNullOrWhiteSpace(error) ? "Extraction failed." : error.Trim();
        ExtractedAt = attemptedAt;
    }

    private static string Required(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", nameof(value))
            : value.Trim();
    }
}
