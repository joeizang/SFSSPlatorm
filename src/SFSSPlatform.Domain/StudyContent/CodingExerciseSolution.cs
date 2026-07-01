namespace SFSSPlatform.Domain.StudyContent;

public sealed class CodingExerciseSolution
{
    private CodingExerciseSolution()
    {
    }

    public CodingExerciseSolution(int codingExerciseId, string code, DateTimeOffset createdAt)
    {
        CodingExerciseId = codingExerciseId;
        Code = Required(code);
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public int Id { get; private set; }

    public int CodingExerciseId { get; private set; }

    public CodingExercise CodingExercise { get; private set; } = null!;

    public string Code { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? LastCheckedAt { get; private set; }

    public void UpdateCode(string code, DateTimeOffset updatedAt)
    {
        Code = Required(code);
        UpdatedAt = updatedAt;
    }

    public void MarkChecked(DateTimeOffset checkedAt)
    {
        LastCheckedAt = checkedAt;
        UpdatedAt = checkedAt;
    }

    private static string Required(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Code is required.", nameof(value))
            : value;
    }
}
