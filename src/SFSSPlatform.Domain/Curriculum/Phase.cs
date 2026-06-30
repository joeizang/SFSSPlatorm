namespace SFSSPlatform.Domain.Curriculum;

public sealed class Phase
{
    private readonly List<PhaseTopic> _topics = [];

    private Phase()
    {
    }

    public Phase(string externalId, string slug, string title, int order, string? description = null)
    {
        ExternalId = Required(externalId);
        Slug = Required(slug);
        Title = Required(title);
        Order = order;
        Description = description;
    }

    public int Id { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int Order { get; private set; }

    public IReadOnlyCollection<PhaseTopic> Topics => _topics;

    public void Sync(string slug, string title, int order, string? description)
    {
        Slug = Required(slug);
        Title = Required(title);
        Order = order;
        Description = description;
    }

    private static string Required(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", nameof(value))
            : value.Trim();
    }
}
