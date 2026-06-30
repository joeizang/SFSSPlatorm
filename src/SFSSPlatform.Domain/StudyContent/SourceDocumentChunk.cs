namespace SFSSPlatform.Domain.StudyContent;

public sealed class SourceDocumentChunk
{
    private SourceDocumentChunk()
    {
    }

    public SourceDocumentChunk(
        int sourceMaterialId,
        int order,
        int startPage,
        int endPage,
        string heading,
        string text)
    {
        SourceMaterialId = sourceMaterialId;
        Order = order;
        StartPage = startPage;
        EndPage = endPage;
        Heading = string.IsNullOrWhiteSpace(heading) ? $"Pages {startPage}-{endPage}" : heading.Trim();
        Text = string.IsNullOrWhiteSpace(text)
            ? throw new ArgumentException("Chunk text is required.", nameof(text))
            : text.Trim();
        CharacterCount = Text.Length;
    }

    public int Id { get; private set; }

    public int SourceMaterialId { get; private set; }

    public SourceMaterial SourceMaterial { get; private set; } = null!;

    public int Order { get; private set; }

    public int StartPage { get; private set; }

    public int EndPage { get; private set; }

    public string Heading { get; private set; } = string.Empty;

    public string Text { get; private set; } = string.Empty;

    public int CharacterCount { get; private set; }
}
