using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed record StudyItemDraft(
    StudyItemKind Kind,
    string Prompt,
    string ExpectedAnswer,
    string Explanation,
    string SourceExcerpt);
