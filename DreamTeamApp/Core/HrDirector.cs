using Nsu.HackathonProblem.Contracts.Models;
using Nsu.HackathonProblem.Contracts.Services;

namespace Nsu.HackathonProblem.Core;

public class HrDirector(IRatingService ratingService,HrManager hrManager)
{
    public void OverseeHackathons(int hackathonCount, List<Employee> juniors,
        List<Employee> teamLeads)
    {
        double totalHarmony = 0;

        for (var i = 1; i <= hackathonCount; i++)
        {
            Console.WriteLine($"--- Hackathon #{i} ---");

            var teams = hrManager.FormTeams(juniors, teamLeads);
            var harmony = CalculateOverallHarmony(teams);

            totalHarmony += harmony;
            Console.WriteLine($"Hackathon Harmony: {harmony:F2}");
        }

        var averageHarmony = totalHarmony / hackathonCount;
        Console.WriteLine(
            $"Average Harmony after {hackathonCount} Hackathons: {averageHarmony:F2}");
    }

    public double CalculateOverallHarmony(List<Team> teams)
    {
       return ratingService.CalculateHarmonicMean(teams);
    }
    
}