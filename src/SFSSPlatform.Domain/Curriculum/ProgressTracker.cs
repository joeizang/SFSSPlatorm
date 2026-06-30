namespace SFSSPlatform.Domain.Curriculum;

public static class ProgressTracker
{
    public static ProgressRollup Calculate(IEnumerable<TopicProgressStatus> statuses)
    {
        var total = 0;
        var done = 0;
        var inProgress = 0;

        foreach (var status in statuses)
        {
            total++;

            if (status == TopicProgressStatus.Done)
            {
                done++;
            }
            else if (status == TopicProgressStatus.InProgress)
            {
                inProgress++;
            }
        }

        return new ProgressRollup(total, done, inProgress);
    }
}
