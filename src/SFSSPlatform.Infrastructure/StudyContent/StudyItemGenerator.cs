using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Domain.StudyContent;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed partial class StudyItemGenerator(StudyPlatformDbContext db)
{
    public async Task<IReadOnlyCollection<StudyItemDraft>> GenerateForChunkAsync(
        int chunkId,
        CancellationToken cancellationToken)
    {
        var chunk = await db.SourceDocumentChunks
            .AsNoTracking()
            .Include(sourceChunk => sourceChunk.SourceMaterial)
            .SingleAsync(sourceChunk => sourceChunk.Id == chunkId, cancellationToken);

        var text = Clean(chunk.Text);
        var paragraphs = SplitParagraphs(text);
        var codeBlocks = ExtractCodeBlocks(text);
        var drafts = new List<StudyItemDraft>();

        drafts.Add(new StudyItemDraft(
            StudyItemKind.Concept,
            $"Explain the main idea of \"{chunk.Heading}\" in your own words.",
            FirstStrongParagraph(paragraphs),
            $"This checks whether you can summarize pages {chunk.StartPage}-{chunk.EndPage} from {chunk.SourceMaterial.Title}.",
            FirstStrongParagraph(paragraphs)));

        foreach (var definition in FindDefinitionParagraphs(paragraphs).Take(2))
        {
            drafts.Add(new StudyItemDraft(
                StudyItemKind.Flashcard,
                BuildDefinitionPrompt(definition),
                definition,
                "Use the source wording to anchor the definition, then rewrite it from memory.",
                definition));
        }

        foreach (var paragraph in paragraphs.Where(IsExplanatoryParagraph).Take(2))
        {
            drafts.Add(new StudyItemDraft(
                StudyItemKind.ShortAnswer,
                BuildWhyHowPrompt(paragraph),
                paragraph,
                "A good answer should identify the cause/effect relationship described in the source.",
                paragraph));
        }

        foreach (var code in codeBlocks.Take(2))
        {
            drafts.Add(new StudyItemDraft(
                StudyItemKind.CodeReading,
                "Read the code snippet. What will it do, return, or print, and why?",
                InferCodeAnswer(code),
                "Focus on control flow, scope, state changes, and the exact output or behavior implied by the snippet.",
                code));
        }

        if (codeBlocks.Count > 0)
        {
            var code = codeBlocks[0];
            drafts.Add(new StudyItemDraft(
                StudyItemKind.CodingExercise,
                BuildCodingExercisePrompt(chunk.Heading, code),
                "Write a working implementation that preserves the behavior demonstrated by the source snippet. Run it and explain the result.",
                "This turns passive code reading into a small implementation exercise tied to the book section.",
                code));
        }

        return drafts
            .Where(IsUsable)
            .DistinctBy(draft => (draft.Kind, NormalizeForKey(draft.Prompt)))
            .Take(8)
            .ToList();
    }

    private static bool IsUsable(StudyItemDraft draft)
    {
        return draft.Prompt.Length >= 20
            && draft.ExpectedAnswer.Length >= 40
            && draft.SourceExcerpt.Length >= 40;
    }

    private static string Clean(string text)
    {
        return text.Replace("\u0008", string.Empty).Trim();
    }

    private static IReadOnlyCollection<string> SplitParagraphs(string text)
    {
        return text.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(paragraph => RepeatedWhitespaceRegex().Replace(paragraph, " ").Trim())
            .Where(paragraph => paragraph.Length is >= 80 and <= 1200)
            .Where(paragraph => !PageMarkerRegex().IsMatch(paragraph))
            .ToList();
    }

    private static string FirstStrongParagraph(IReadOnlyCollection<string> paragraphs)
    {
        return paragraphs.FirstOrDefault(IsExplanatoryParagraph)
            ?? paragraphs.FirstOrDefault()
            ?? "Review the selected source chunk and identify the most important idea before answering.";
    }

    private static IEnumerable<string> FindDefinitionParagraphs(IEnumerable<string> paragraphs)
    {
        return paragraphs.Where(paragraph =>
            DefinitionRegex().IsMatch(paragraph)
            || paragraph.Contains(" is ", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains(" are ", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsExplanatoryParagraph(string paragraph)
    {
        return paragraph.Contains("because", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains("therefore", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains("this means", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains("this is", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains("when ", StringComparison.OrdinalIgnoreCase)
            || paragraph.Contains("if ", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildDefinitionPrompt(string paragraph)
    {
        var subject = paragraph.Split(['.', ':', ';'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(subject) || subject.Length > 140)
        {
            return "Define the concept described in the source excerpt.";
        }

        return $"Define or explain this concept: {subject}";
    }

    private static string BuildWhyHowPrompt(string paragraph)
    {
        var trimmed = paragraph.Length > 180 ? $"{paragraph[..180].Trim()}..." : paragraph;
        return $"Why does this matter, and how would you apply it? Source claim: {trimmed}";
    }

    private static IReadOnlyList<string> ExtractCodeBlocks(string text)
    {
        var lines = text.Split('\n');
        var blocks = new List<string>();
        var buffer = new List<string>();

        foreach (var line in lines)
        {
            if (LooksLikeCode(line))
            {
                buffer.Add(line.TrimEnd());
                continue;
            }

            if (buffer.Count >= 3)
            {
                blocks.Add(string.Join(Environment.NewLine, buffer).Trim());
            }

            buffer.Clear();
        }

        if (buffer.Count >= 3)
        {
            blocks.Add(string.Join(Environment.NewLine, buffer).Trim());
        }

        return blocks
            .Where(block => block.Count(char.IsLetterOrDigit) >= 20)
            .Distinct()
            .ToList();
    }

    private static bool LooksLikeCode(string line)
    {
        var value = line.Trim();
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Length > 180 && !value.Contains('{') && !value.Contains(';')) return false;

        return value.StartsWith("function ", StringComparison.Ordinal)
            || value.StartsWith("public ", StringComparison.Ordinal)
            || value.StartsWith("private ", StringComparison.Ordinal)
            || value.StartsWith("class ", StringComparison.Ordinal)
            || value.StartsWith("using ", StringComparison.Ordinal)
            || value.StartsWith("const ", StringComparison.Ordinal)
            || value.StartsWith("let ", StringComparison.Ordinal)
            || value.StartsWith("var ", StringComparison.Ordinal)
            || value.StartsWith("if ", StringComparison.Ordinal)
            || value.StartsWith("for ", StringComparison.Ordinal)
            || value.StartsWith("while ", StringComparison.Ordinal)
            || value.StartsWith("console.", StringComparison.Ordinal)
            || value is "}" or "{" or ");"
            || value.EndsWith(';')
            || value.EndsWith('{')
            || value.EndsWith('}');
    }

    private static string InferCodeAnswer(string code)
    {
        if (code.Contains("console.log", StringComparison.Ordinal))
        {
            return "Trace each console.log call in order. Pay attention to variable scope, assignments, and whether any line throws before later statements run.";
        }

        if (code.Contains("return ", StringComparison.Ordinal))
        {
            return "Trace the function inputs and return path. The answer should state the returned value and why that branch is reached.";
        }

        return "Explain the snippet line by line, naming the state changes, scope rules, side effects, and final observable behavior.";
    }

    private static string BuildCodingExercisePrompt(string heading, string code)
    {
        var firstLine = code.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        return string.IsNullOrWhiteSpace(firstLine)
            ? $"Create a small runnable example that demonstrates the idea from \"{heading}\"."
            : $"Recreate and modify this example from \"{heading}\" so it still demonstrates the same rule: {firstLine}";
    }

    private static string NormalizeForKey(string value)
    {
        return RepeatedWhitespaceRegex().Replace(value.Trim().ToLowerInvariant(), " ");
    }

    [GeneratedRegex(@"\b(is|are|means|refers to|is called|are called)\b", RegexOptions.IgnoreCase)]
    private static partial Regex DefinitionRegex();

    [GeneratedRegex(@"\[\s*\d+\s*\]|^Chapter\s+\d+", RegexOptions.IgnoreCase)]
    private static partial Regex PageMarkerRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex RepeatedWhitespaceRegex();
}
