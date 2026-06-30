namespace SFSSPlatform.Domain.StudyContent;

public sealed class StudyAttempt
{
    private StudyAttempt()
    {
    }

    public StudyAttempt(
        int studyItemId,
        string answer,
        StudyAttemptRating rating,
        DateTimeOffset attemptedAt)
    {
        StudyItemId = studyItemId;
        Answer = string.IsNullOrWhiteSpace(answer) ? string.Empty : answer.Trim();
        Rating = rating;
        AttemptedAt = attemptedAt;
    }

    public int Id { get; private set; }

    public int StudyItemId { get; private set; }

    public StudyItem StudyItem { get; private set; } = null!;

    public string Answer { get; private set; } = string.Empty;

    public StudyAttemptRating Rating { get; private set; }

    public DateTimeOffset AttemptedAt { get; private set; }
}
