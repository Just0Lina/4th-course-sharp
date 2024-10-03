using DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.Database;
using Nsu.HackathonProblem.Models;

namespace Nsu.HackathonProblem.Core;

public class HrDirector(
    IRatingService ratingService,
    HrManager hrManager,
    HackathonDbContext context)
{
    public async Task OverseeHackathons(int hackathonCount,
        List<Employee> juniors,
        List<Employee> teamLeads)
    {
        double totalHarmony = 0;

        for (var i = 1; i <= hackathonCount; i++)
        {
            Console.WriteLine($"--- Hackathon #{i} ---");

            var (juniorPreferences, teamLeadPreferences) =
                hrManager.GetPreferences(juniors, teamLeads);
            var teams =
                hrManager.FormTeams(juniorPreferences, teamLeadPreferences);

            var harmony = CalculateOverallHarmony(teams);

            totalHarmony += harmony;

            var hackathon = new HackathonEntity
            {
                Harmony = (decimal)harmony,
                Teams = teams.Select(t => new TeamEntity
                {
                    TeamLeadId = t.TeamLead.Id,
                    JuniorId = t.Junior.Id
                }).ToList()
            };


            context.Hackathons.Add(hackathon);
            await context.SaveChangesAsync();
            await SavePreferencesAsync(juniorPreferences, Role.Junior,
                hackathon.Id);
            await SavePreferencesAsync(teamLeadPreferences, Role.TeamLead,
                hackathon.Id);

            Console.WriteLine($"Hackathon Harmony: {harmony:F2}");
        }

        var averageHarmony = totalHarmony / hackathonCount;
        Console.WriteLine(
            $"Average Harmony after {hackathonCount} Hackathons: {averageHarmony:F2}");
    }


    private async Task SavePreferencesAsync(
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


    public double CalculateOverallHarmony(List<Team> teams)
    {
        return ratingService.CalculateHarmonicMean(teams);
    }

    public async Task PrintHackathonResultsAsync(int hackathonId)
    {
        var hackathon = await context.Hackathons
            .Include(h => h.Teams)
            .FirstOrDefaultAsync(h => h.Id == hackathonId);

        if (hackathon == null)
        {
            Console.WriteLine($"No Hackathon found with ID {hackathonId}");
            return;
        }

        Console.WriteLine($"Hackathon ID: {hackathon.Id}");
        Console.WriteLine($"Harmony: {hackathon.Harmony:F2}");

        var participants = new HashSet<string>();

        foreach (var team in hackathon.Teams)
        {
            var teamLeadName =
                await GetEmployeeByIdAsync(team.TeamLeadId, Role.TeamLead);
            var juniorName =
                await GetEmployeeByIdAsync(team.JuniorId, Role.Junior);

            Console.WriteLine(
                $"Team Lead: {teamLeadName}, Junior: {juniorName}");

            participants.Add(teamLeadName);
            participants.Add(juniorName);
        }

        Console.WriteLine("\nParticipants:");
        foreach (var participant in participants)
        {
            Console.WriteLine($"- {participant}");
        }
    }


    private async Task<string> GetEmployeeByIdAsync(int employeeId, Role role)
    {
        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.Role == role);

        return employee?.Name ?? "Unknown";
    }

    public async Task PrintAverageHarmonyAsync()
    {
        var hackathons = await context.Hackathons.ToListAsync();

        if (!hackathons.Any())
        {
            Console.WriteLine("No hackathons found in the database.");
            return;
        }

        var averageHarmony = hackathons.Average(h => h.Harmony);
        Console.WriteLine(
            $"Average Harmony across all hackathons: {averageHarmony:F2}");
    }


    public async Task<decimal> CalculateAverageHarmonyAsync()
    {
        var averageHarmony = await context.Hackathons
            .AverageAsync(h => h.Harmony);

        Console.WriteLine(
            $"Average Harmony across all hackathons: {averageHarmony:F2}");
        return averageHarmony;
    }
}