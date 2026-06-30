using TaskType = SFSSPlatform.Domain.Curriculum.TaskType;

namespace SFSSPlatform.Infrastructure.Curriculum;

public sealed record CurriculumSeed(
    IReadOnlyCollection<ModuleSeed> Modules,
    IReadOnlyCollection<PhaseSeed> Phases);

public sealed record ModuleSeed(
    string Id,
    string? Slug,
    string Title,
    string? Description,
    int Order,
    IReadOnlyCollection<TopicSeed> Topics);

public sealed record TopicSeed(
    string Id,
    string? Slug,
    string Title,
    string? Summary,
    int Order,
    IReadOnlyCollection<TaskType> TaskTypes);

public sealed record PhaseSeed(
    string Id,
    string? Slug,
    string Title,
    string? Description,
    int Order,
    IReadOnlyCollection<PhaseTopicSeed> Topics);

public sealed record PhaseTopicSeed(string TopicId, int Order);
