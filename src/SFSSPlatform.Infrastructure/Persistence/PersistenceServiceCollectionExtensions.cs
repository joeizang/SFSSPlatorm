using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFSSPlatform.Infrastructure.Curriculum;
using SFSSPlatform.Infrastructure.StudyContent;

namespace SFSSPlatform.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddStudyPlatformPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=sfssplatform.db";

        services.AddDbContext<StudyPlatformDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<CurriculumImporter>();
        services.Configure<LocalPdfIngestionOptions>(options =>
        {
            options.SourceFolder = configuration["LocalSources:SourceFolder"];
            if (int.TryParse(configuration["LocalSources:PagesPerChunk"], out var pagesPerChunk))
            {
                options.PagesPerChunk = pagesPerChunk;
            }
        });
        services.AddScoped<LocalPdfIngestionService>();
        services.AddScoped<StudyItemGenerator>();

        return services;
    }
}
