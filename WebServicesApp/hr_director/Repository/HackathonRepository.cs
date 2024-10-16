using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.HrDirector.Database;
using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Repository;

public class HackathonRepository(HackathonDbContext context)
    : IHackathonRepository
{
    public async Task AddHackathonAsync(HackathonEntity hackathon)
    {
        try
        {
            Console.WriteLine("Adding hackathon");
            context.Hackathons.Add(hackathon);


            await context.SaveChangesAsync();
            Console.WriteLine($"Inserted Hackathon ID: {hackathon.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving hackathon: {ex.Message}");
        }
    }


    public async Task SavePreferencesAsync(List<Wishlist> preferences,
        Role role, int hackathonId)
    {
        foreach (var preference in preferences)
        {
            for (var index = 0;
                 index < preference.DesiredEmployees.Length;
                 index++)
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

    public async Task SaveEmployeesAsync(List<Team> teams)
    {
        foreach (var team in teams)
        {
            var teamLead = new EmployeeEntity(team.TeamLead.Id, team.TeamLead.Name, Role.TeamLead);
            var junior = new EmployeeEntity(team.Junior.Id, team.Junior.Name, Role.Junior);

            var existingTeamLead = await context.Employees.FindAsync(teamLead.Id, Role.TeamLead);
            if (existingTeamLead == null)
            {
                context.Employees.Add(teamLead);
            }
           

            var existingJunior = await context.Employees.FindAsync(junior.Id,Role.Junior);
            if (existingJunior == null)
            {
                context.Employees.Add(junior);
            }
           
        }

        try
        {
            await context.SaveChangesAsync();
            Console.WriteLine("Employees saved successfully.");
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"An error occurred while saving changes: {dbEx.Message}");
            if (dbEx.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

}