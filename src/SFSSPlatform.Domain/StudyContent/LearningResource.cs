namespace SFSSPlatform.Domain.StudyContent;

public sealed class LearningResource
{
    private LearningResource()
    {
    }

    public LearningResource(
        string externalId,
        LearningResourceProvider provider,
        string title,
        string creator,
        string url,
        string embedUrl,
        string summary,
        string tags,
        int? durationSeconds,
        DateTimeOffset createdAt)
    {
        ExternalId = Required(externalId);
        Provider = provider;
        Title = Required(title);
        Creator = Required(creator);
        Url = Required(url);
        EmbedUrl = Required(embedUrl);
        Summary = Required(summary);
        Tags = Required(tags);
        DurationSeconds = durationSeconds;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public int Id { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public LearningResourceProvider Provider { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Creator { get; private set; } = string.Empty;

    public string Url { get; private set; } = string.Empty;

    public string EmbedUrl { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string Tags { get; private set; } = string.Empty;

    public int? DurationSeconds { get; private set; }

    public int? TopicId { get; private set; }

    public int? SourceMaterialId { get; private set; }

    public SourceMaterial? SourceMaterial { get; private set; }

    public int? SourceDocumentChunkId { get; private set; }

    public SourceDocumentChunk? SourceDocumentChunk { get; private set; }

    public bool IsWatched { get; private set; }

    public DateTimeOffset? WatchedAt { get; private set; }

    public int WatchProgressSeconds { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void SyncMetadata(
        string title,
        string creator,
        string url,
        string embedUrl,
        string summary,
        string tags,
        int? durationSeconds,
        DateTimeOffset updatedAt)
    {
        Title = Required(title);
        Creator = Required(creator);
        Url = Required(url);
        EmbedUrl = Required(embedUrl);
        Summary = Required(summary);
        Tags = Required(tags);
        DurationSeconds = durationSeconds;
        UpdatedAt = updatedAt;
    }

    public void AttachToSource(int? sourceMaterialId, int? sourceDocumentChunkId, DateTimeOffset updatedAt)
    {
        SourceMaterialId = sourceMaterialId;
        SourceDocumentChunkId = sourceDocumentChunkId;
        UpdatedAt = updatedAt;
    }

    public void UpdateWatchState(
        bool isWatched,
        int watchProgressSeconds,
        string notes,
        DateTimeOffset updatedAt)
    {
        IsWatched = isWatched;
        WatchedAt = isWatched ? updatedAt : null;
        WatchProgressSeconds = Math.Max(0, watchProgressSeconds);
        Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim();
        UpdatedAt = updatedAt;
    }

    private static string Required(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", nameof(value))
            : value.Trim();
    }
}
