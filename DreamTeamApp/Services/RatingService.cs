using Nsu.HackathonProblem.Contracts.Models;

namespace Nsu.HackathonProblem.Contracts.Services;

public class RatingService : IRatingService
{
    public double CalculateHarmonicMean(List<Team> teams)
    {
        var n = teams.Count * 2;

        var sumOfReciprocals = (from team in teams
            let teamLeadSatisfaction = team.TeamLeadPriority
            let juniorSatisfaction = team.JuniorPriority
            select (1.0 / teamLeadSatisfaction) +
                   (1.0 / juniorSatisfaction)).Sum();

        return n / sumOfReciprocals;
    }
}