using Microsoft.EntityFrameworkCore;
using SFSSPlatform.Infrastructure.Persistence;

namespace SFSSPlatform.Api.Infrastructure;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyPlatformDbContext>();
        await db.Database.MigrateAsync();
    }
}
