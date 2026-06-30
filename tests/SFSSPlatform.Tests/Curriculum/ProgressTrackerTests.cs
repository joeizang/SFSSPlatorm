using SFSSPlatform.Domain.Curriculum;

namespace SFSSPlatform.Tests.Curriculum;

public sealed class ProgressTrackerTests
{
    [Fact]
    public void Calculate_returns_zero_completion_for_empty_input()
    {
        var rollup = ProgressTracker.Calculate([]);

        Assert.Equal(0, rollup.TotalTopics);
        Assert.Equal(0, rollup.CompletionPercentage);
    }

    [Fact]
    public void Calculate_counts_done_and_in_progress_topics()
    {
        var rollup = ProgressTracker.Calculate(
            [
                TopicProgressStatus.Done,
                TopicProgressStatus.InProgress,
                TopicProgressStatus.NotStarted,
                TopicProgressStatus.Done
            ]);

        Assert.Equal(4, rollup.TotalTopics);
        Assert.Equal(2, rollup.DoneTopics);
        Assert.Equal(1, rollup.InProgressTopics);
        Assert.Equal(50m, rollup.CompletionPercentage);
    }
}
