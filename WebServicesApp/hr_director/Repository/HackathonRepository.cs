using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Nsu.HackathonProblem.HrDirector.Database;
using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Repository;

public class HackathonRepository : IHackathonRepository
{
    private ConcurrentBag<Wishlist> _juniorPreferences = new();
    private ConcurrentBag<Wishlist> _teamLeadPreferences = new();
    public event Action OnAllPreferencesReceived;
    private int hackathonId;
    private readonly ILogger<HackathonRepository> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory1;

    public HackathonRepository(ILogger<HackathonRepository> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory1 = serviceScopeFactory;
        OnAllPreferencesReceived += async () =>
        {
            await SavePreferenceAsync();
        };
    }
    

    public async Task UpdateHackathonAsync(decimal harmony,
        List<TeamEntity> teams, int hackathonId)
    {
        using var scope = _serviceScopeFactory1.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        var hackathon = await GetLatestHackathonAsync(hackathonId, db);
        if (hackathon == null)
        {
            throw new InvalidOperationException(
                $"Hackathon with ID {hackathonId} not found.");
        }
        _logger.LogInformation($"hackathonId: {hackathonId}, harmonyIndex: {harmony}");

        hackathon.Teams.Clear();
        hackathon.Teams.AddRange(teams);
        hackathon.Teams = teams;
        hackathon.Harmony = harmony;

        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Wishlist>> GetJuniorWishlistsAsync(
        int hackathonId)
    {
        using var scope = _serviceScopeFactory1.CreateScope();
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
        using var scope = _serviceScopeFactory1.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        return await db.EmployeePreferences
            .Where(p => p.HackathonId == hackathonId && p.Role == role)
            .CountAsync();
    }


    public async Task ClearPreferencesAndTeamsForHackathonIdAsync(
        int hackathonId)
    {
        using var scope = _serviceScopeFactory1.CreateScope();
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
        using var scope = _serviceScopeFactory1.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();


        var teamLeadPreferences = await db.EmployeePreferences
            .Where(ep =>
                ep.Role == Role.TeamLead && ep.HackathonId == hackathonId)
            .ToListAsync();

        return teamLeadPreferences
            .GroupBy(p => p.EmployeeId)
            .Select(MapToWishlist);
    }


    public async Task SaveJuniorPreferences(int hackathonId, Wishlist wishlist)
    {
        _juniorPreferences.Add(wishlist);
        this.hackathonId = hackathonId;
        CheckIfAllPreferencesReceived();
    }

    public async Task SaveTeamLeadPreferences(int hackathonId,
        Wishlist wishlist)
    {
        _teamLeadPreferences.Add(wishlist);
        this.hackathonId = hackathonId;
        CheckIfAllPreferencesReceived();
    }

    public bool AllRequestsReceived()
    {
        return _juniorPreferences.Count >= 5 &&
               _teamLeadPreferences.Count >= 5;
    }

    private void CheckIfAllPreferencesReceived()
    {
        _logger.LogInformation(
            $"Junior Preferences: {_juniorPreferences.Count}, Team Lead Preferences: {_teamLeadPreferences.Count}");

        if (_juniorPreferences.Count < 5 ||
            _teamLeadPreferences.Count < 5) return;

        _logger.LogInformation(
            "All preferences received, triggering OnAllPreferencesReceived event.");
        OnAllPreferencesReceived?.Invoke();
    }


    private async Task SavePreferenceAsync()
    {
        using var scope = _serviceScopeFactory1.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();

        var uniquePreferences =
            new HashSet<(int EmployeeId, int PreferredEmployeeId, Role Role)>();
        var preferences = new List<EmployeePreferenceEntity>();
        foreach (var preference in _juniorPreferences)
        {
            var role = Role.Junior;
            for (var index = 0;
                 index < preference.DesiredEmployees.Length;
                 index++)
            {
                await SavePreferenses(preference, index, role, db,
                    uniquePreferences, preferences);
            }
        }

        foreach (var preference in _teamLeadPreferences)
        {
            var role = Role.TeamLead;
            for (var index = 0;
                 index < preference.DesiredEmployees.Length;
                 index++)
            {
                await SavePreferenses(preference, index, role, db,
                    uniquePreferences, preferences);
            }
        }

        try
        {
            foreach (var pref in preferences)
            {
                if (!db.Entry(pref).State.HasFlag(EntityState.Detached))
                {
                    db.Entry(pref).State =
                        EntityState
                            .Added;
                }
            }

            if (preferences.Count > 0)
            {
                foreach (var preference1 in preferences)
                {
                    _logger.LogInformation(
                        $"Preferences added: {preference1.EmployeeId} {preference1.PreferredEmployeeId} {preference1.Role}");
                }
                await db.EmployeePreferences.AddRangeAsync(preferences);
                await db.SaveChangesAsync();
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Entity tracking issue: {Message}", ex.Message);
        }
    }

    private async Task SavePreferenses(Wishlist preference, int index,
        Role role,
        HackathonDbContext? db,
        HashSet<(int EmployeeId, int PreferredEmployeeId, Role Role)>
            uniquePreferences, List<EmployeePreferenceEntity> preferences)
    {
        var pref = preference.DesiredEmployees[index];
        var key = (preference.EmployeeId, pref, role);
        var existingPreference = await db.EmployeePreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(ep =>
                ep.EmployeeId == preference.EmployeeId &&
                ep.HackathonId == hackathonId &&
                ep.PreferredEmployeeId == pref &&
                ep.Role == role);
        if (!uniquePreferences.Contains(key) &&
            existingPreference == null)
        {
            uniquePreferences.Add(key);

            preferences.Add(new EmployeePreferenceEntity
            {
                HackathonId = hackathonId,
                EmployeeId = preference.EmployeeId,
                PreferredEmployeeId = pref,
                Priority = index,
                Role = role
            });

            _logger.LogInformation(
                $"Adding preference for EmployeeId: {hackathonId} {role} {preference.EmployeeId}, PreferredEmployeeId: {pref}, Priority: {index}");
        }
    }


    public async Task<HackathonEntity?> GetHackathonByIdAsync(int hackathonId)
    {
        using var scope = _serviceScopeFactory1.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        return await db.Hackathons
            .Include(h => h.Teams)
            .FirstOrDefaultAsync(h => h.Id == hackathonId);
    }

    public async Task<IEnumerable<HackathonEntity>> GetAllHackathonsAsync()
    {
        using var scope = _serviceScopeFactory1.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        return await db.Hackathons.ToListAsync();
    }

    public async Task<double> CalculateAverageHarmonyAsync()
    {
        using var scope = _serviceScopeFactory1.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        var harmonies = await db.Hackathons
            .Select(h => h.Harmony)
            .ToListAsync();

        return harmonies.DefaultIfEmpty(0).Average(h => (double)h);
    }

    public async Task SaveEmployeesAsync(List<Team> teams)
    {
        using var scope = _serviceScopeFactory1.CreateScope();
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
        using var scope = _serviceScopeFactory1.CreateScope();
        var db = scope.ServiceProvider.GetService<HackathonDbContext>();
        var hackathonEntity = new HackathonEntity { Id = hackathonId };
        db.Hackathons.Add(hackathonEntity);
        await db.SaveChangesAsync();
    }

    public async Task<HackathonEntity> GetLatestHackathonAsync(int hackathonId, HackathonDbContext db)
    {
        return await db.Hackathons
            .Include(h => h.Teams)
            .FirstOrDefaultAsync(h => h.Id == hackathonId); }
}