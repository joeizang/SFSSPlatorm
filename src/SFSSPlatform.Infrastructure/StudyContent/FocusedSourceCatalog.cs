using SFSSPlatform.Domain.StudyContent;

namespace SFSSPlatform.Infrastructure.StudyContent;

public static class FocusedSourceCatalog
{
    private static readonly Dictionary<string, SourceAccess> AccessByFileName = new(StringComparer.OrdinalIgnoreCase)
    {
        ["asp-net-core-6-succinctly.pdf"] = SourceAccess.Open,
        ["RustProgrammingLanguage2E.pdf"] = SourceAccess.Open,
    };

    public static readonly IReadOnlySet<string> FileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "ASP.NET_Core_in_Action_Third_Edition.pdf",
        "9781835888926-REAL_WORLD_WEB_DEVELOPMENT_WITH_NET_10.pdf",
        "9781805123385-ARCHITECTING_ASPNET_CORE_APPLICATIONS.pdf",
        "Building_Web_APIs_with_ASP.NET_Core.pdf",
        "9781837632121-ASPNET_8_BEST_PRACTICES.pdf",
        "Pro_ASP.NET_Core_7_Tenth_Edition.pdf",
        "ASP.NET_Core_Security.pdf",
        "Entity Framework Core in Action by Jon Smith (z-lib.org).pdf",
        "9781836206637-C_14_AND_NET_10_MODERN_CROSS_PLATFORM_DEVELOPMENT_FUNDAMENTALS.pdf",
        "9781800564718-HIGHPERFORMANCE_PROGRAMMING_IN_C_AND_NET.pdf",
        "Joe Mayo - C# Cookbook_ Modern Recipes for Professional Developers-O'Reilly Media (2021).pdf",
        "Mikael Olsson - C# 10 Quick Syntax Reference_ A Pocket Guide to the Language, APIs, and Library-Apress (2022).pdf",
        "Simple and Efficient Programming with C#.pdf",
        "9781836643173-LEARN_REACT_WITH_TYPESCRIPT.pdf",
        "David Griffiths, Dawn Griffiths - React Cookbook_ Recipes for Mastering the React Framework-O'Reilly Media (2021).pdf",
        "John Larsen - React Hooks in Action_ With Suspense and Concurrent Mode-Manning Publications (2021).pdf",
        "Adam Freeman - Essential TypeScript 4_ From Beginner to Pro-Apress (2021).pdf",
        "Adam Boduch, Roy Derks, Mikhail Sakhniuk - React and React Native_ Build cross-platform JavaScript applications with native power for the web, desktop, and mobile-Packt Publishing (2022).pdf",
        "David Flanagan - JavaScript_ The Definitive Guide_ Master the World's Most-Used Programming Language-O'Reilly Media, Inc. (2020).pdf",
        "Adam D. Scott, Matthew MacDonald, Shelley Powers - JavaScript Cookbook_ Programming the Web-O'Reilly Media (2021).pdf",
        "9781804615072-MASTERING_NODEJS_WEB_DEVELOPMENT.pdf",
        "9781804612279-JAVASCRIPT_DESIGN_PATTERNS.pdf",
        "9781800562523-JAVASCRIPT_FROM_BEGINNER_TO_PROFESSIONAL.pdf",
        "9781835080917-PRACTICAL_HTML_AND_CSS.pdf",
        "9781837028238-RESPONSIVE_WEB_DESIGN_WITH_HTML5_AND_CSS.pdf",
        "Grokking_Algorithms_Second_Edition.pdf",
        "Marcello La Rocca - Advanced Algorithms and Data Structures-Manning Publications (2021).pdf",
        "Algorithms_Nutshell .pdf",
    };

    public static SourceAccess GetAccess(string fileName)
    {
        return AccessByFileName.TryGetValue(fileName, out var access)
            ? access
            : SourceAccess.PurchasedLocal;
    }
}
