using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.HrDirector.Database;
using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Repository;

public class HackathonRepository(
    ILogger<HackathonRepository> _logger,
    IServiceScopeFactory _serviceScopeFactory)
    : IHackathonRepository
{
    public async Task AddHackathonAsync(HackathonEntity hackathon)
    {
        try
        {
            _logger.LogInformation("Adding hackathon");
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetService<HackathonDbContext>();
            db.Hackathons.Add(hackathon);
            await db.SaveChangesAsync();
            _logger.LogInformation($"Inserted Hackathon ID: {hackathon.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Error saving hackathon: {ex.Message}");
        }
    }

    public async Task UpdateHackathonAsync(decimal harmony,
        List<TeamEntity> teams, int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        var hackathon = await GetLatestHackathonAsync(hackathonId);
        if (hackathon == null)
        {
            throw new InvalidOperationException($"Hackathon with ID {hackathonId} not found.");
        }
        if (hackathon.Teams == null)
        {
            hackathon.Teams = new List<TeamEntity>();
        }
        hackathon.Teams = teams;
        hackathon.Harmony = harmony;

        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Wishlist>> GetJuniorWishlistsAsync(
        int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();

        var juniorPreferences = await db.EmployeePreferences
            .Where(ep =>
                ep.Role == Role.Junior && ep.HackathonId == hackathonId)
            .ToListAsync();

        return juniorPreferences.GroupBy(p => p.EmployeeId)
            .Select(MapToWishlist);
    }

    public async Task<int> GetPreferencesCountAsync(int hackathonId, Role role)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        return await db.EmployeePreferences
            .Where(p => p.HackathonId == hackathonId && p.Role == role)
            .CountAsync();
    }
    

    public async Task ClearPreferencesAndTeamsForHackathonIdAsync(
        int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        var preferences =
            db.EmployeePreferences.Where(p => p.HackathonId == hackathonId);
        db.EmployeePreferences.RemoveRange(preferences);

        var teams = db.Teams.Where(t => t.HackathonId == hackathonId);
        db.Teams.RemoveRange(teams);

        await db.SaveChangesAsync();
    }

    private Wishlist MapToWishlist(
        IEnumerable<EmployeePreferenceEntity> preferences)
    {
        if (preferences == null || !preferences.Any())
        {
            return new Wishlist(EmployeeId: 0,
                DesiredEmployees: Array.Empty<int>());
        }

        var employeeId = preferences.First().EmployeeId;

        var desiredEmployees = preferences
            .Select(p => p.PreferredEmployeeId)
            .ToArray();

        return new Wishlist(EmployeeId: employeeId,
            DesiredEmployees: desiredEmployees);
    }

    public async Task<IEnumerable<Wishlist>> GetTeamLeadWishlistsAsync(
        int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();


        var teamLeadPreferences = await db.EmployeePreferences
            .Where(ep =>
                ep.Role == Role.TeamLead && ep.HackathonId == hackathonId)
            .ToListAsync();

        return teamLeadPreferences
            .GroupBy(p => p.EmployeeId)
            .Select(MapToWishlist);
    }


    public async Task SavePreferenceAsync(Role role, int hackathonId,
        Wishlist preference)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        for (var index = 0; index < preference.DesiredEmployees.Length; index++)
        {
            var pref = preference.DesiredEmployees[index];
            var preferenceEntity = new EmployeePreferenceEntity
            {
                HackathonId = hackathonId,
                EmployeeId = preference.EmployeeId,
                PreferredEmployeeId = pref,
                Priority = index,
                Role = role
            };

            db.EmployeePreferences.Add(preferenceEntity);
            Console.WriteLine(
                $"Added preference for EmployeeId: {hackathonId} {role} {preference.EmployeeId}, PreferredEmployeeId: {pref}, Priority: {index}");
        }

        await db.SaveChangesAsync();
    }

    public async Task<HackathonEntity?> GetHackathonByIdAsync(int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        return await db.Hackathons
            .Include(h => h.Teams)
            .FirstOrDefaultAsync(h => h.Id == hackathonId);
    }

    public async Task<IEnumerable<HackathonEntity>> GetAllHackathonsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        return await db.Hackathons.ToListAsync();
    }

    public async Task<double> CalculateAverageHarmonyAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        var harmonies = await db.Hackathons
            .Select(h => h.Harmony)
            .ToListAsync();

        return harmonies.DefaultIfEmpty(0).Average(h => (double)h);
    }

    public async Task SaveEmployeesAsync(List<Team> teams)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        foreach (var team in teams)
        {
            var teamLead = new EmployeeEntity(team.TeamLead.Id,
                team.TeamLead.Name, Role.TeamLead);
            var junior = new EmployeeEntity(team.Junior.Id, team.Junior.Name,
                Role.Junior);

            var existingTeamLead =
                await db.Employees.FindAsync(teamLead.Id, Role.TeamLead);
            if (existingTeamLead == null)
            {
                db.Employees.Add(teamLead);
            }


            var existingJunior =
                await db.Employees.FindAsync(junior.Id, Role.Junior);
            if (existingJunior == null)
            {
                db.Employees.Add(junior);
            }
        }

        try
        {
            await db.SaveChangesAsync();
            _logger.LogInformation("Employees saved successfully.");
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogInformation(
                $"An error occurred while saving changes: {dbEx.Message}");
            if (dbEx.InnerException != null)
            {
                _logger.LogInformation(
                    $"Inner exception: {dbEx.InnerException.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(
                $"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task SaveHackathonIdAsync(int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        var hackathonEntity = new HackathonEntity { Id = hackathonId };
        db.Hackathons.Add(hackathonEntity);
        await db.SaveChangesAsync();
    }
    
    public async Task<HackathonEntity> GetLatestHackathonAsync(int hackathonId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        return await db.Hackathons
            .Include(h => h.Teams)
            .FirstOrDefaultAsync(h => h.Id == hackathonId);
    }
}