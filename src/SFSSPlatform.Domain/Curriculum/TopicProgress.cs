namespace SFSSPlatform.Domain.Curriculum;

public sealed record TopicProgress(
    TopicProgressStatus Status,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt)
{
    public static TopicProgress NotStarted()
    {
        return new TopicProgress(TopicProgressStatus.NotStarted, DateTimeOffset.UnixEpoch, null, null);
    }

    public TopicProgress WithStatus(TopicProgressStatus status, DateTimeOffset now)
    {
        return status switch
        {
            TopicProgressStatus.NotStarted => new TopicProgress(status, now, null, null),
            TopicProgressStatus.InProgress => new TopicProgress(status, now, StartedAt ?? now, null),
            TopicProgressStatus.Done => new TopicProgress(status, now, StartedAt ?? now, now),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown topic progress status.")
        };
    }
}
