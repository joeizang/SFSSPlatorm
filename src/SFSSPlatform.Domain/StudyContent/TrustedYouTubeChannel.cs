namespace SFSSPlatform.Domain.StudyContent;

public sealed class TrustedYouTubeChannel
{
    private TrustedYouTubeChannel()
    {
    }

    public TrustedYouTubeChannel(
        string name,
        string url,
        string tags,
        TrustedChannelPriority priority,
        string notes,
        DateTimeOffset createdAt)
    {
        Name = Required(name);
        Url = Required(url);
        Tags = Required(tags);
        Priority = priority;
        Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim();
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Url { get; private set; } = string.Empty;

    public string Tags { get; private set; } = string.Empty;

    public TrustedChannelPriority Priority { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void SyncMetadata(
        string name,
        string tags,
        TrustedChannelPriority priority,
        string notes,
        DateTimeOffset updatedAt)
    {
        Name = Required(name);
        Tags = Required(tags);
        Priority = priority;
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
