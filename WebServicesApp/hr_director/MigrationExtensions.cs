using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.HrDirector.Database;

namespace Nsu.HackathonProblem.HrDirector;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext =
            scope.ServiceProvider.GetRequiredService<HackathonDbContext>();
        dbContext.Database.Migrate();
    }
}