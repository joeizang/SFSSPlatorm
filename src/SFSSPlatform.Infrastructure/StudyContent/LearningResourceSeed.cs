using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed record LearningResourceSeed(
    string ExternalId,
    LearningResourceProvider Provider,
    string Title,
    string Creator,
    string Url,
    string EmbedUrl,
    string Summary,
    string Tags,
    int? DurationSeconds);
