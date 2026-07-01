using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Api.Features.StudyItems;

public sealed class CSharpExerciseAnalyzer
{
    private static readonly Regex PackageSectionRegex = new(
        @"(?<manager>NuGet|NPM)\s*:\s*(?<value>.*?)(?=(?:\.\s*(?:NuGet|NPM)\s*:)|$)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly ConcurrentDictionary<string, IReadOnlyCollection<MetadataReference>> ReferenceCache = new();

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ExerciseCheckResponse Analyze(CodingExercise exercise, string code, DateTimeOffset checkedAt)
    {
        var packageRequirements = ParsePackageRequirements(exercise.PackageRequirements);
        var checkDefinition = ParseCheckDefinition(exercise.CheckDefinitionJson);

        if (!IsCSharp(exercise.Language))
        {
            var unsupportedResult = new ExerciseGradingResponse(
                "unsupported",
                0,
                0,
                [
                    new ExerciseRuleCheckResponse(
                        "language",
                        "C# analyzer support",
                        "This exercise language is not currently supported by the Roslyn checker.",
                        "fail",
                        "Use a C# exercise for Roslyn-backed checks.")
                ],
                checkDefinition.SampleCases);

            return new ExerciseCheckResponse(
                false,
                [
                    new ExerciseDiagnosticResponse(
                        "SFSS0001",
                        "error",
                        $"Roslyn diagnostics currently support C# exercises only. This exercise is marked as '{exercise.Language}'.",
                        1,
                        1,
                        1,
                        1)
                ],
                packageRequirements,
                unsupportedResult,
                checkedAt);
        }

        var parseOptions = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Latest);
        var syntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions);
        var compilation = CSharpCompilation.Create(
            $"SFSSExercise_{exercise.Id}",
            [syntaxTree],
            GetPlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable)
                .WithOptimizationLevel(OptimizationLevel.Debug));

        var diagnostics = compilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
            .Select(ToResponse)
            .OrderBy(diagnostic => diagnostic.StartLine)
            .ThenBy(diagnostic => diagnostic.StartColumn)
            .ToList();
        var grading = Grade(compilation, syntaxTree, diagnostics, checkDefinition);

        return new ExerciseCheckResponse(
            diagnostics.All(diagnostic => diagnostic.Severity != "error") && grading.RequiredFailed == 0,
            diagnostics,
            packageRequirements,
            grading,
            checkedAt);
    }

    public IReadOnlyCollection<PackageRequirementResponse> ParsePackageRequirements(string requirements)
    {
        if (string.IsNullOrWhiteSpace(requirements))
        {
            return [];
        }

        var packages = new List<PackageRequirementResponse>();
        foreach (Match match in PackageSectionRegex.Matches(requirements))
        {
            var manager = NormalizeManager(match.Groups["manager"].Value);
            var value = match.Groups["value"].Value.Trim();
            var status = value.Contains("none", StringComparison.OrdinalIgnoreCase)
                ? "notRequired"
                : "declared";

            foreach (var packageName in SplitPackageNames(value))
            {
                packages.Add(new PackageRequirementResponse(manager, packageName, status));
            }
        }

        return packages;
    }

    private static ExerciseGradingResponse Grade(
        CSharpCompilation compilation,
        SyntaxTree syntaxTree,
        IReadOnlyCollection<ExerciseDiagnosticResponse> diagnostics,
        ExerciseCheckDefinition checkDefinition)
    {
        var root = syntaxTree.GetRoot();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var checks = new List<ExerciseRuleCheckResponse>();
        var compilePassed = diagnostics.All(diagnostic => diagnostic.Severity != "error");
        checks.Add(Check(
            "compile",
            "Compiles as a C# library",
            "The submitted code must compile with the configured platform references.",
            compilePassed,
            compilePassed ? "No compiler errors were found." : "Fix compiler errors before contract checks can be trusted."));

        var exerciseClass = root
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(type => type.Identifier.ValueText == "Exercise");
        checks.Add(Check(
            "contract.class",
            "Defines Exercise class",
            "The grader expects a public static Exercise class.",
            exerciseClass is not null && HasModifier(exerciseClass.Modifiers, SyntaxKind.PublicKeyword) && HasModifier(exerciseClass.Modifiers, SyntaxKind.StaticKeyword),
            "Declare public static class Exercise."));

        var runMethod = exerciseClass?
            .Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.ValueText == "RunAsync");
        checks.Add(Check(
            "contract.method",
            "Defines RunAsync",
            "The exercise entry point must be public static RunAsync.",
            runMethod is not null && HasModifier(runMethod.Modifiers, SyntaxKind.PublicKeyword) && HasModifier(runMethod.Modifiers, SyntaxKind.StaticKeyword),
            "Add public static Task<string> RunAsync(...)."));

        var signaturePassed = HasExpectedRunAsyncSignature(semanticModel, runMethod);
        checks.Add(Check(
            "contract.signature",
            "Preserves method signature",
            "Keep public static Task<string> RunAsync(string input, CancellationToken cancellationToken = default).",
            signaturePassed,
            "Use Task<string> return type, a string input parameter, and a CancellationToken parameter."));

        var rejectsInvalidInput = runMethod is not null && RejectsInvalidInput(runMethod);
        checks.Add(Check(
            "behavior.invalid-input",
            "Rejects invalid input",
            "Guard null, empty, or whitespace input and throw ArgumentException.",
            rejectsInvalidInput,
            "Check string.IsNullOrWhiteSpace(input) or equivalent and throw ArgumentException."));

        var observesCancellation = runMethod is not null && ObservesCancellationToken(runMethod);
        checks.Add(Check(
            "behavior.cancellation",
            "Observes cancellation",
            "Accept and observe the CancellationToken inside RunAsync.",
            observesCancellation,
            "Call cancellationToken.ThrowIfCancellationRequested(), pass the token to async APIs, or otherwise observe it."));

        var replacedPlaceholder = runMethod is not null && ReplacedStarterPlaceholder(runMethod);
        checks.Add(Check(
            "behavior.implementation",
            "Replaces starter placeholder",
            "Avoid returning the starter expression unchanged.",
            replacedPlaceholder,
            "Replace the placeholder return with topic-specific implementation behavior."));

        var requiredFailed = checks.Count(check => check.Status == "fail");
        var passed = checks.Count(check => check.Status == "pass");
        var status = requiredFailed == 0 ? "passed" : compilePassed ? "needsWork" : "blocked";

        return new ExerciseGradingResponse(status, passed, requiredFailed, checks, checkDefinition.SampleCases);
    }

    private static IReadOnlyCollection<MetadataReference> GetPlatformReferences()
    {
        return ReferenceCache.GetOrAdd("trusted-platform-assemblies", _ =>
        {
            var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
            if (string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
            {
                return [];
            }

            return trustedPlatformAssemblies
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(path => MetadataReference.CreateFromFile(path))
                .ToList();
        });
    }

    private static ExerciseDiagnosticResponse ToResponse(Diagnostic diagnostic)
    {
        var span = diagnostic.Location.IsInSource
            ? diagnostic.Location.GetLineSpan()
            : default;
        var start = diagnostic.Location.IsInSource ? span.StartLinePosition : default;
        var end = diagnostic.Location.IsInSource ? span.EndLinePosition : start;

        return new ExerciseDiagnosticResponse(
            diagnostic.Id,
            diagnostic.Severity == DiagnosticSeverity.Error ? "error" : "warning",
            diagnostic.GetMessage(),
            start.Line + 1,
            start.Character + 1,
            Math.Max(end.Line + 1, start.Line + 1),
            Math.Max(end.Character + 1, start.Character + 1));
    }

    private static ExerciseRuleCheckResponse Check(
        string id,
        string title,
        string description,
        bool passed,
        string guidance)
    {
        return new ExerciseRuleCheckResponse(
            id,
            title,
            description,
            passed ? "pass" : "fail",
            passed ? "Good." : guidance);
    }

    private static bool HasExpectedRunAsyncSignature(SemanticModel semanticModel, MethodDeclarationSyntax? method)
    {
        if (method is null || method.ParameterList.Parameters.Count != 2)
        {
            return false;
        }

        var symbol = semanticModel.GetDeclaredSymbol(method);
        if (symbol is null || symbol.ReturnType.ToDisplayString() != "System.Threading.Tasks.Task<string>")
        {
            return false;
        }

        var firstParameter = symbol.Parameters[0];
        var secondParameter = symbol.Parameters[1];
        return firstParameter is { Name: "input" }
            && firstParameter.Type.SpecialType == SpecialType.System_String
            && secondParameter is { Name: "cancellationToken" }
            && secondParameter.Type.ToDisplayString() == "System.Threading.CancellationToken";
    }

    private static bool RejectsInvalidInput(MethodDeclarationSyntax method)
    {
        var methodText = method.ToFullString();
        return (methodText.Contains("IsNullOrWhiteSpace", StringComparison.Ordinal)
                || methodText.Contains("IsNullOrEmpty", StringComparison.Ordinal))
            && methodText.Contains("ArgumentException", StringComparison.Ordinal);
    }

    private static bool ObservesCancellationToken(MethodDeclarationSyntax method)
    {
        var tokens = method
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Count(identifier => identifier.Identifier.ValueText == "cancellationToken");

        return tokens > 1
            || method.ToFullString().Contains("ThrowIfCancellationRequested", StringComparison.Ordinal);
    }

    private static bool ReplacedStarterPlaceholder(MethodDeclarationSyntax method)
    {
        var methodText = method.ToFullString();
        return !methodText.Contains("TODO", StringComparison.OrdinalIgnoreCase)
            && !methodText.Contains("return Task.FromResult(input.Trim());", StringComparison.Ordinal);
    }

    private static bool HasModifier(SyntaxTokenList modifiers, SyntaxKind syntaxKind)
    {
        return modifiers.Any(modifier => modifier.IsKind(syntaxKind));
    }

    private static ExerciseCheckDefinition ParseCheckDefinition(string checkDefinitionJson)
    {
        if (string.IsNullOrWhiteSpace(checkDefinitionJson) || checkDefinitionJson.Trim() == "{}")
        {
            return new ExerciseCheckDefinition(string.Empty, [], []);
        }

        try
        {
            return JsonSerializer.Deserialize<ExerciseCheckDefinition>(checkDefinitionJson, JsonOptions)
                ?? new ExerciseCheckDefinition(string.Empty, [], []);
        }
        catch (JsonException)
        {
            return new ExerciseCheckDefinition(string.Empty, [], []);
        }
    }

    private static string NormalizeManager(string manager)
    {
        return manager.Equals("npm", StringComparison.OrdinalIgnoreCase) ? "npm" : "nuget";
    }

    private static IReadOnlyCollection<string> SplitPackageNames(string value)
    {
        if (value.Contains("none", StringComparison.OrdinalIgnoreCase))
        {
            return ["none"];
        }

        return value
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(package => package.Trim().TrimEnd('.'))
            .Where(package => package.Length > 0)
            .ToList();
    }

    private static bool IsCSharp(string language)
    {
        var normalized = language.Trim().ToLowerInvariant();
        return normalized is "csharp" or "c#";
    }
}

public sealed record AnalyzeExerciseCodeRequest(string Code);

public sealed record SaveCodingExerciseSolutionRequest(string Code);

public sealed record CodingExerciseSolutionResponse(
    int ExerciseId,
    string TopicSlug,
    string Code,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastCheckedAt);

public sealed record ExerciseDiagnosticResponse(
    string Id,
    string Severity,
    string Message,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn);

public sealed record PackageRequirementResponse(
    string Manager,
    string Name,
    string Status);

public sealed record ExerciseCheckDefinition(
    string Summary,
    IReadOnlyCollection<ExerciseVisibleCheckDefinition> VisibleChecks,
    IReadOnlyCollection<ExerciseSampleCaseResponse> SampleCases);

public sealed record ExerciseVisibleCheckDefinition(
    string Id,
    string Title,
    string Description);

public sealed record ExerciseSampleCaseResponse(
    string Name,
    string Input,
    string? Expected = null,
    string? ExpectedException = null);

public sealed record ExerciseRuleCheckResponse(
    string Id,
    string Title,
    string Description,
    string Status,
    string Guidance);

public sealed record ExerciseGradingResponse(
    string Status,
    int Passed,
    int RequiredFailed,
    IReadOnlyCollection<ExerciseRuleCheckResponse> Checks,
    IReadOnlyCollection<ExerciseSampleCaseResponse> SampleCases);

public sealed record ExerciseCheckResponse(
    bool Succeeded,
    IReadOnlyCollection<ExerciseDiagnosticResponse> Diagnostics,
    IReadOnlyCollection<PackageRequirementResponse> PackageRequirements,
    ExerciseGradingResponse Grading,
    DateTimeOffset CheckedAt);
