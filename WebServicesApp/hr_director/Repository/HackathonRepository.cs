using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.HrDirector.Database;
using Nsu.HackathonProblem.HrDirector.Repository;
using Nsu.HackathonProblem.SharedData.Models;

public class HackathonRepository(
    IServiceScopeFactory _serviceScopeFactory,
    ILogger<HackathonRepository> logger)
    : IHackathonRepository
{

    public async Task UpdateHackathonAsync(decimal harmony,
        List<TeamEntity> teams, int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        var hackathon = await GetLatestHackathonAsync(hackathonId, dbContext);
        if (hackathon == null)
        {
            throw new InvalidOperationException(
                $"Hackathon with ID {hackathonId} not found.");
        }

        logger.LogInformation(
            $"hackathonId: {hackathonId}, harmonyIndex: {harmony}");

        hackathon.Teams.Clear();
        hackathon.Teams.AddRange(teams);
        hackathon.Harmony = harmony;

        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Wishlist>> GetJuniorWishlistsAsync(int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        var juniorPreferences = await dbContext.EmployeePreferences
            .Where(ep => ep.Role == Role.Junior && ep.HackathonId == hackathonId)
            .ToListAsync();

        return juniorPreferences.GroupBy(p => p.EmployeeId)
            .Select(MapToWishlist);
    }

    public async Task<int> GetPreferencesCountAsync(int hackathonId, Role role)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        return await dbContext.EmployeePreferences
            .Where(p => p.HackathonId == hackathonId && p.Role == role)
            .CountAsync();
    }

    public async Task ClearPreferencesAndTeamsForHackathonIdAsync(int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        var preferences = dbContext.EmployeePreferences.Where(p => p.HackathonId == hackathonId);
        dbContext.EmployeePreferences.RemoveRange(preferences);

        var teams = dbContext.Teams.Where(t => t.HackathonId == hackathonId);
        dbContext.Teams.RemoveRange(teams);

        await dbContext.SaveChangesAsync();
    }

    private Wishlist MapToWishlist(IEnumerable<EmployeePreferenceEntity> preferences)
    {
        if (preferences == null || !preferences.Any())
        {
            return new Wishlist(EmployeeId: 0, DesiredEmployees: Array.Empty<int>());
        }

        var employeeId = preferences.First().EmployeeId;
        var desiredEmployees = preferences.Select(p => p.PreferredEmployeeId).ToArray();

        return new Wishlist(EmployeeId: employeeId, DesiredEmployees: desiredEmployees);
    }

    public async Task<IEnumerable<Wishlist>> GetTeamLeadWishlistsAsync(int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        var teamLeadPreferences = await dbContext.EmployeePreferences
            .Where(ep => ep.Role == Role.TeamLead && ep.HackathonId == hackathonId)
            .ToListAsync();

        return teamLeadPreferences.GroupBy(p => p.EmployeeId)
            .Select(MapToWishlist);
    }

    public async Task<HackathonEntity?> GetHackathonByIdAsync(int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        return await dbContext.Hackathons
            .Include(h => h.Teams)
            .FirstOrDefaultAsync(h => h.Id == hackathonId);
    }

    public async Task<IEnumerable<HackathonEntity>> GetAllHackathonsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        return await dbContext.Hackathons.ToListAsync();
    }

    public async Task<double> CalculateAverageHarmonyAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        var harmonies = await dbContext.Hackathons
            .Select(h => h.Harmony)
            .ToListAsync();

        return harmonies.DefaultIfEmpty(0).Average(h => (double)h);
    }

    public async Task SavePreferencesToDatabaseAsync(PreferencesMessage message)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        for (int priority = 0; priority < message.Preferences.DesiredEmployees.Length; priority++)
        {
            var preferenceEntity = new EmployeePreferenceEntity
            {
                HackathonId = message.HackathonId,
                Role = message.EmployeeType == "junior" ? Role.Junior : Role.TeamLead,
                EmployeeId = message.Employee.Id,
                PreferredEmployeeId = message.Preferences.DesiredEmployees[priority],
                Priority = priority
            };

            dbContext.EmployeePreferences.Add(preferenceEntity);
        }

        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogDebug(
                $"Saved preferences for HackathonId: {message.HackathonId}, EmployeeId: {message.Employee.Id}, EmployeeType: {message.EmployeeType}");
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogError($"An error occurred while saving preferences: {dbEx.Message}");
            if (dbEx.InnerException != null)
            {
                logger.LogError($"Inner exception: {dbEx.InnerException.Message}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task SaveEmployeesAsync(List<Team> teams)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        foreach (var team in teams)
        {
            var teamLead = new EmployeeEntity(team.TeamLead.Id, team.TeamLead.Name, Role.TeamLead);
            var junior = new EmployeeEntity(team.Junior.Id, team.Junior.Name, Role.Junior);

            var existingTeamLead = await dbContext.Employees.FindAsync(teamLead.Id, Role.TeamLead);
            if (existingTeamLead == null)
            {
                dbContext.Employees.Add(teamLead);
            }

            var existingJunior = await dbContext.Employees.FindAsync(junior.Id, Role.Junior);
            if (existingJunior == null)
            {
                dbContext.Employees.Add(junior);
            }
        }

        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Employees saved successfully.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogError($"An error occurred while saving changes: {dbEx.Message}");
            if (dbEx.InnerException != null)
            {
                logger.LogError($"Inner exception: {dbEx.InnerException.Message}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task SaveHackathonIdAsync(int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        var hackathonEntity = new HackathonEntity { Id = hackathonId };
        dbContext.Hackathons.Add(hackathonEntity);
        await dbContext.SaveChangesAsync();
    }

    private async Task<HackathonEntity> GetLatestHackathonAsync(int hackathonId, HackathonDbContext dbContext)
    {
        return await dbContext.Hackathons
                   .Include(h => h.Teams)
                   .FirstOrDefaultAsync(h => h.Id == hackathonId) ??
               throw new InvalidOperationException();
    }
}
