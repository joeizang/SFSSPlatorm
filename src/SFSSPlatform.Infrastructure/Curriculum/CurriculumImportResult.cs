namespace SFSSPlatform.Infrastructure.Curriculum;

public sealed record CurriculumImportResult(
    int ModulesProcessed,
    int TopicsProcessed,
    int PhasesProcessed,
    int PhaseTopicLinksProcessed);
