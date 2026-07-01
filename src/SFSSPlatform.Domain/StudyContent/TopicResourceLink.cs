using SFSSPlatform.Domain.Curriculum;

namespace SFSSPlatform.Domain.StudyContent;

public sealed class TopicResourceLink
{
    private TopicResourceLink()
    {
    }

    public TopicResourceLink(
        int topicId,
        int? learningResourceId,
        int? videoCandidateId,
        int priority,
        string notes,
        DateTimeOffset createdAt)
    {
        if ((learningResourceId is null) == (videoCandidateId is null))
        {
            throw new ArgumentException("A topic resource link requires exactly one learning resource or video candidate.");
        }

        TopicId = topicId;
        LearningResourceId = learningResourceId;
        VideoCandidateId = videoCandidateId;
        Priority = NormalizePriority(priority);
        Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim();
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public int Id { get; private set; }

    public int TopicId { get; private set; }

    public Topic Topic { get; private set; } = null!;

    public int? LearningResourceId { get; private set; }

    public LearningResource? LearningResource { get; private set; }

    public int? VideoCandidateId { get; private set; }

    public VideoCandidate? VideoCandidate { get; private set; }

    public int Priority { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(int priority, string notes, DateTimeOffset updatedAt)
    {
        Priority = NormalizePriority(priority);
        Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim();
        UpdatedAt = updatedAt;
    }

    private static int NormalizePriority(int priority)
    {
        return Math.Clamp(priority, 1, 5);
    }
}
