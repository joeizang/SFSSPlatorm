namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed record VideoCandidateImportResult(
    string SourceFile,
    int CandidatesDiscovered,
    int CandidatesCreated,
    int CandidatesUpdated,
    int CandidatesSkipped);
