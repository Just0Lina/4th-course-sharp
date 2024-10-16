using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.Database;
using Nsu.HackathonProblem.Models;

namespace Nsu.HackathonProblem.Repositories;

public class HackathonRepository(HackathonDbContext context)
    : IHackathonRepository
{
    public async Task AddHackathonAsync(HackathonEntity hackathon)
    {
        context.Hackathons.Add(hackathon);
        await context.SaveChangesAsync();
    }


    public async Task SavePreferencesAsync(
        List<EmployeePreferences> preferences, Role role, int hackathonId)
    {
        foreach (var preference in preferences)
        {
            foreach (var pref in preference.PreferredEmployees)
            {
                var preferenceEntity = new EmployeePreferenceEntity
                {
                    HackathonId = hackathonId,
                    EmployeeId = preference.Employee.Id,
                    PreferredEmployeeId = pref.Key.Id,
                    Priority = pref.Value,
                    Role = role
                };

                context.EmployeePreferences.Add(preferenceEntity);
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task<HackathonEntity> GetHackathonByIdAsync(int hackathonId)
    {
        return await context.Hackathons
            .Include(h => h.Teams)
            .FirstOrDefaultAsync(h => h.Id == hackathonId);
    }

    public async Task<IEnumerable<HackathonEntity>> GetAllHackathonsAsync()
    {
        return await context.Hackathons.ToListAsync();
    }

    public async Task<double> CalculateAverageHarmonyAsync()
    {
        var harmonies = await context.Hackathons
            .Select(h => h.Harmony)
            .ToListAsync();

        return harmonies.DefaultIfEmpty(0).Average(h => (double)h);
    }
}