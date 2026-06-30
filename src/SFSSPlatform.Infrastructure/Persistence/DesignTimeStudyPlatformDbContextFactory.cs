using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SFSSPlatform.Infrastructure.Persistence;

public sealed class DesignTimeStudyPlatformDbContextFactory
    : IDesignTimeDbContextFactory<StudyPlatformDbContext>
{
    public StudyPlatformDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StudyPlatformDbContext>()
            .UseSqlite("Data Source=sfssplatform.db")
            .Options;

        return new StudyPlatformDbContext(options);
    }
}
