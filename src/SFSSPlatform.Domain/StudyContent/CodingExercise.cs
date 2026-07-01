using SFSSPlatform.Domain.Curriculum;

namespace SFSSPlatform.Domain.StudyContent;

public sealed class CodingExercise
{
    private CodingExercise()
    {
    }

    public CodingExercise(
        int topicId,
        string title,
        string prompt,
        CodingExerciseDifficulty difficulty,
        string language,
        string starterCode,
        string packageRequirements,
        string successCriteria,
        string hints,
        string checkDefinitionJson,
        DateTimeOffset createdAt)
    {
        TopicId = topicId;
        Title = Required(title);
        Prompt = Required(prompt);
        Difficulty = difficulty;
        Language = Required(language);
        StarterCode = Required(starterCode);
        PackageRequirements = string.IsNullOrWhiteSpace(packageRequirements) ? string.Empty : packageRequirements.Trim();
        SuccessCriteria = Required(successCriteria);
        Hints = string.IsNullOrWhiteSpace(hints) ? string.Empty : hints.Trim();
        CheckDefinitionJson = string.IsNullOrWhiteSpace(checkDefinitionJson) ? "{}" : checkDefinitionJson.Trim();
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public int Id { get; private set; }

    public int TopicId { get; private set; }

    public Topic Topic { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public string Prompt { get; private set; } = string.Empty;

    public CodingExerciseDifficulty Difficulty { get; private set; }

    public string Language { get; private set; } = string.Empty;

    public string StarterCode { get; private set; } = string.Empty;

    public string PackageRequirements { get; private set; } = string.Empty;

    public string SuccessCriteria { get; private set; } = string.Empty;

    public string Hints { get; private set; } = string.Empty;

    public string CheckDefinitionJson { get; private set; } = "{}";

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string title,
        string prompt,
        CodingExerciseDifficulty difficulty,
        string language,
        string starterCode,
        string packageRequirements,
        string successCriteria,
        string hints,
        string checkDefinitionJson,
        DateTimeOffset updatedAt)
    {
        Title = Required(title);
        Prompt = Required(prompt);
        Difficulty = difficulty;
        Language = Required(language);
        StarterCode = Required(starterCode);
        PackageRequirements = string.IsNullOrWhiteSpace(packageRequirements) ? string.Empty : packageRequirements.Trim();
        SuccessCriteria = Required(successCriteria);
        Hints = string.IsNullOrWhiteSpace(hints) ? string.Empty : hints.Trim();
        CheckDefinitionJson = string.IsNullOrWhiteSpace(checkDefinitionJson) ? "{}" : checkDefinitionJson.Trim();
        UpdatedAt = updatedAt;
    }

    private static string Required(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", nameof(value))
            : value.Trim();
    }
}
