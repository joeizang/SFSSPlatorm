namespace SFSSPlatform.Domain.Curriculum;

public sealed class TopicNote
{
    private TopicNote()
    {
    }

    public TopicNote(int topicId, string content, DateTimeOffset createdAt)
    {
        TopicId = topicId;
        Content = NormalizeContent(content);
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public int Id { get; private set; }

    public int TopicId { get; private set; }

    public Topic Topic { get; private set; } = null!;

    public string Content { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string content, DateTimeOffset updatedAt)
    {
        Content = NormalizeContent(content);
        UpdatedAt = updatedAt;
    }

    private static string NormalizeContent(string content)
    {
        return string.IsNullOrWhiteSpace(content) ? string.Empty : content.Trim();
    }
}
