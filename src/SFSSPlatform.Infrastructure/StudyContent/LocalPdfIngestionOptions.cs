namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed class LocalPdfIngestionOptions
{
    public string? SourceFolder { get; set; }

    public int PagesPerChunk { get; set; } = 4;
}
