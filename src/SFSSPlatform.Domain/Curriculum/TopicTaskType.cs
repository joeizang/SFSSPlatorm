namespace SFSSPlatform.Domain.Curriculum;

public sealed class TopicTaskType
{
    private TopicTaskType()
    {
    }

    public TopicTaskType(int topicId, TaskType taskType)
    {
        TopicId = topicId;
        TaskType = taskType;
    }

    public int TopicId { get; private set; }

    public Topic Topic { get; private set; } = null!;

    public TaskType TaskType { get; private set; }
}
