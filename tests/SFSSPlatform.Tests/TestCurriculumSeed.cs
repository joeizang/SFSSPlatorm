using SFSSPlatform.Infrastructure.Curriculum;
using TaskType = SFSSPlatform.Domain.Curriculum.TaskType;

namespace SFSSPlatform.Tests;

internal static class TestCurriculumSeed
{
    public static CurriculumSeed Create()
    {
        return new CurriculumSeed(
            [
                new ModuleSeed(
                    "module-aspnet-core",
                    "aspnet-core",
                    "ASP.NET Core",
                    "Production API development with ASP.NET Core.",
                    1,
                    [
                        new TopicSeed(
                            "topic-minimal-api-routing",
                            "minimal-api-routing",
                            "Minimal API routing",
                            "Build stable HTTP contracts with route groups and typed results.",
                            1,
                            [TaskType.Read, TaskType.Build, TaskType.WriteTests]),
                        new TopicSeed(
                            "topic-problemdetails",
                            "problem-details",
                            "ProblemDetails",
                            "Return consistent error payloads from API boundaries.",
                            2,
                            [TaskType.Read, TaskType.Review])
                    ]),
                new ModuleSeed(
                    "module-react",
                    "react-typescript",
                    "React + TypeScript",
                    "Type-safe frontend development.",
                    2,
                    [
                        new TopicSeed(
                            "topic-query-cache",
                            "tanstack-query-cache",
                            "TanStack Query cache",
                            "Model server state explicitly in a React app.",
                            1,
                            [TaskType.Read, TaskType.Build])
                    ])
            ],
            [
                new PhaseSeed(
                    "phase-one",
                    "phase-one",
                    "Phase One",
                    "Foundation path.",
                    1,
                    [
                        new PhaseTopicSeed("topic-minimal-api-routing", 1),
                        new PhaseTopicSeed("topic-query-cache", 2)
                    ])
            ]);
    }
}
