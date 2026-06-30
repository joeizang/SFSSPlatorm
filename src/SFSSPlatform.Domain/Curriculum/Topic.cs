namespace SFSSPlatform.Domain.Curriculum;

public sealed class Topic
{
    private readonly List<TopicTaskType> _taskTypes = [];

    private Topic()
    {
    }

    public Topic(
        string externalId,
        string slug,
        string title,
        int moduleId,
        int order,
        string? summary = null)
    {
        ExternalId = Required(externalId);
        Slug = Required(slug);
        Title = Required(title);
        ModuleId = moduleId;
        Order = order;
        Summary = summary;
        Progress = TopicProgress.NotStarted();
    }

    public int Id { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Summary { get; private set; }

    public int Order { get; private set; }

    public int ModuleId { get; private set; }

    public CurriculumModule Module { get; private set; } = null!;

    public TopicProgress Progress { get; private set; } = TopicProgress.NotStarted();

    public IReadOnlyCollection<TopicTaskType> TaskTypes => _taskTypes;

    public void Sync(string slug, string title, int moduleId, int order, string? summary)
    {
        Slug = Required(slug);
        Title = Required(title);
        ModuleId = moduleId;
        Order = order;
        Summary = summary;
    }

    public void SetProgress(TopicProgressStatus status, DateTimeOffset now)
    {
        Progress = Progress.WithStatus(status, now);
    }

    private static string Required(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", nameof(value))
            : value.Trim();
    }
}
