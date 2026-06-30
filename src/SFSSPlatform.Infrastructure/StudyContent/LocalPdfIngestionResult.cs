namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed record LocalPdfIngestionResult(
    int SourcesDiscovered,
    int SourcesIngested,
    int SourcesSkipped,
    int SourcesFailed,
    int ChunksCreated);
