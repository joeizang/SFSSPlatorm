using SFSSPlatform.Domain.Curriculum;
using TaskType = SFSSPlatform.Domain.Curriculum.TaskType;

namespace SFSSPlatform.Api.Features.Catalog;

public sealed record CatalogRollupsResponse(
    IReadOnlyCollection<PhaseRollupResponse> Phases,
    IReadOnlyCollection<ModuleRollupResponse> Modules);

public sealed record ModuleResponse(
    string Slug,
    string Title,
    string? Description,
    int TotalTopics,
    int DoneTopics,
    IReadOnlyCollection<ModuleTopicResponse> Topics)
{
    public decimal CompletionPercentage =>
        TotalTopics == 0 ? 0 : Math.Round(DoneTopics * 100m / TotalTopics, 2);
}

public sealed record ModuleRollupResponse(
    string Slug,
    string Title,
    int TotalTopics,
    int DoneTopics,
    int InProgressTopics)
{
    public decimal CompletionPercentage =>
        TotalTopics == 0 ? 0 : Math.Round(DoneTopics * 100m / TotalTopics, 2);
}

public sealed record ModuleTopicResponse(
    string Slug,
    string Title,
    TopicProgressStatus Status);

public sealed record PhaseResponse(
    string Slug,
    string Title,
    string? Description,
    IReadOnlyCollection<PhaseTopicResponse> Topics);

public sealed record PhaseRollupResponse(
    string Slug,
    string Title,
    int TotalTopics,
    int DoneTopics,
    int InProgressTopics)
{
    public decimal CompletionPercentage =>
        TotalTopics == 0 ? 0 : Math.Round(DoneTopics * 100m / TotalTopics, 2);
}

public sealed record PhaseTopicResponse(
    string Slug,
    string Title,
    string ModuleSlug,
    string ModuleTitle,
    TopicProgressStatus Status);

public sealed record TopicDetailResponse(
    string Slug,
    string Title,
    string? Summary,
    string ModuleSlug,
    string ModuleTitle,
    TopicProgressStatus Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<TaskType> TaskTypes);

public sealed record TopicSearchResponse(
    string Slug,
    string Title,
    string? Summary,
    string ModuleSlug,
    string ModuleTitle,
    TopicProgressStatus Status,
    IReadOnlyCollection<TaskType> TaskTypes);
