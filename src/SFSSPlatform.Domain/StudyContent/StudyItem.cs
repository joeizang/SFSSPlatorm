namespace SFSSPlatform.Domain.StudyContent;

public sealed class StudyItem
{
    private StudyItem()
    {
    }

    public StudyItem(
        int sourceMaterialId,
        int sourceDocumentChunkId,
        StudyItemKind kind,
        string prompt,
        string expectedAnswer,
        string explanation,
        string sourceExcerpt,
        int startPage,
        int endPage,
        DateTimeOffset createdAt)
    {
        SourceMaterialId = sourceMaterialId;
        SourceDocumentChunkId = sourceDocumentChunkId;
        Kind = kind;
        Prompt = Required(prompt);
        ExpectedAnswer = Required(expectedAnswer);
        Explanation = Required(explanation);
        SourceExcerpt = Required(sourceExcerpt);
        StartPage = startPage;
        EndPage = endPage;
        Status = StudyItemStatus.Active;
        ConfidenceScore = 0;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public int Id { get; private set; }

    public int SourceMaterialId { get; private set; }

    public SourceMaterial SourceMaterial { get; private set; } = null!;

    public int SourceDocumentChunkId { get; private set; }

    public SourceDocumentChunk SourceDocumentChunk { get; private set; } = null!;

    public StudyItemKind Kind { get; private set; }

    public string Prompt { get; private set; } = string.Empty;

    public string ExpectedAnswer { get; private set; } = string.Empty;

    public string Explanation { get; private set; } = string.Empty;

    public string SourceExcerpt { get; private set; } = string.Empty;

    public int StartPage { get; private set; }

    public int EndPage { get; private set; }

    public StudyItemStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public int ConfidenceScore { get; private set; }

    public DateTimeOffset? LastAttemptAt { get; private set; }

    public DateTimeOffset? NextReviewAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        StudyItemKind kind,
        string prompt,
        string expectedAnswer,
        string explanation,
        StudyItemStatus status,
        DateTimeOffset updatedAt)
    {
        Kind = kind;
        Prompt = Required(prompt);
        ExpectedAnswer = Required(expectedAnswer);
        Explanation = Required(explanation);
        Status = status;
        UpdatedAt = updatedAt;
    }

    public void RecordAttempt(StudyAttemptRating rating, DateTimeOffset attemptedAt)
    {
        AttemptCount++;
        LastAttemptAt = attemptedAt;
        ConfidenceScore = rating switch
        {
            StudyAttemptRating.Again => Math.Max(0, ConfidenceScore - 2),
            StudyAttemptRating.Hard => Math.Max(0, ConfidenceScore),
            StudyAttemptRating.Good => Math.Min(10, ConfidenceScore + 1),
            StudyAttemptRating.Easy => Math.Min(10, ConfidenceScore + 2),
            _ => ConfidenceScore
        };
        NextReviewAt = attemptedAt.Add(ReviewInterval(rating, ConfidenceScore));
        UpdatedAt = attemptedAt;
    }

    private static TimeSpan ReviewInterval(StudyAttemptRating rating, int confidenceScore)
    {
        return rating switch
        {
            StudyAttemptRating.Again => TimeSpan.FromMinutes(10),
            StudyAttemptRating.Hard => TimeSpan.FromHours(6),
            StudyAttemptRating.Good => TimeSpan.FromDays(Math.Max(1, confidenceScore)),
            StudyAttemptRating.Easy => TimeSpan.FromDays(Math.Max(2, confidenceScore * 2)),
            _ => TimeSpan.FromDays(1)
        };
    }

    private static string Required(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", nameof(value))
            : value.Trim();
    }
}
