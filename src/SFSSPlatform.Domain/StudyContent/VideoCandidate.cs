namespace SFSSPlatform.Domain.StudyContent;

public sealed class VideoCandidate
{
    private VideoCandidate()
    {
    }

    public VideoCandidate(
        string externalId,
        string title,
        string channelName,
        string channelUrl,
        string url,
        string embedUrl,
        string summary,
        string tags,
        VideoCandidateDifficulty difficulty,
        string notes,
        DateTimeOffset createdAt)
    {
        ExternalId = Required(externalId);
        Title = Required(title);
        ChannelName = Required(channelName);
        ChannelUrl = Required(channelUrl);
        Url = Required(url);
        EmbedUrl = Required(embedUrl);
        Summary = Required(summary);
        Tags = Required(tags);
        Difficulty = difficulty;
        Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim();
        Status = VideoCandidateStatus.Candidate;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public int Id { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string ChannelName { get; private set; } = string.Empty;

    public string ChannelUrl { get; private set; } = string.Empty;

    public string Url { get; private set; } = string.Empty;

    public string EmbedUrl { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string Tags { get; private set; } = string.Empty;

    public VideoCandidateDifficulty Difficulty { get; private set; }

    public VideoCandidateStatus Status { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public string RejectionReason { get; private set; } = string.Empty;

    public int? LearningResourceId { get; private set; }

    public LearningResource? LearningResource { get; private set; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    public DateTimeOffset? RejectedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void SyncMetadata(
        string title,
        string channelName,
        string channelUrl,
        string url,
        string embedUrl,
        string summary,
        string tags,
        VideoCandidateDifficulty difficulty,
        string notes,
        DateTimeOffset updatedAt)
    {
        Title = Required(title);
        ChannelName = Required(channelName);
        ChannelUrl = Required(channelUrl);
        Url = Required(url);
        EmbedUrl = Required(embedUrl);
        Summary = Required(summary);
        Tags = Required(tags);
        Difficulty = difficulty;
        Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim();
        UpdatedAt = updatedAt;
    }

    public void Accept(int learningResourceId, DateTimeOffset acceptedAt)
    {
        LearningResourceId = learningResourceId;
        Status = VideoCandidateStatus.Accepted;
        AcceptedAt = acceptedAt;
        RejectedAt = null;
        RejectionReason = string.Empty;
        UpdatedAt = acceptedAt;
    }

    public void Reject(string reason, DateTimeOffset rejectedAt)
    {
        Status = VideoCandidateStatus.Rejected;
        RejectedAt = rejectedAt;
        RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Not a fit for the current study catalog." : reason.Trim();
        UpdatedAt = rejectedAt;
    }

    private static string Required(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", nameof(value))
            : value.Trim();
    }
}
