namespace SFSSPlatform.Domain.Curriculum;

public sealed record ProgressRollup(int TotalTopics, int DoneTopics, int InProgressTopics)
{
    public decimal CompletionPercentage =>
        TotalTopics == 0 ? 0 : Math.Round(DoneTopics * 100m / TotalTopics, 2);
}
