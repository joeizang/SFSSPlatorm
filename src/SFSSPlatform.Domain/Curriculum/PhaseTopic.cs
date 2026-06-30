namespace SFSSPlatform.Domain.Curriculum;

public sealed class PhaseTopic
{
    private PhaseTopic()
    {
    }

    public PhaseTopic(int phaseId, int topicId, int order)
    {
        PhaseId = phaseId;
        TopicId = topicId;
        Order = order;
    }

    public int PhaseId { get; private set; }

    public Phase Phase { get; private set; } = null!;

    public int TopicId { get; private set; }

    public Topic Topic { get; private set; } = null!;

    public int Order { get; private set; }

    public void SyncOrder(int order)
    {
        Order = order;
    }
}
