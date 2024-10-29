using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.HrDirector.Database;

namespace Nsu.HackathonProblem.HrDirector;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var dbContext =
            scope.ServiceProvider.GetRequiredService<HackathonDbContext>();
        dbContext.Database.Migrate();
    }
}