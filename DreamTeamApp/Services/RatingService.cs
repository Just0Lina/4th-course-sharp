
namespace Nsu.HackathonProblem.Contracts.Services
{
    using System.Collections.Generic;
    using Nsu.HackathonProblem.Contracts.Models;
    public class RatingService
    {

        public double CalculateHarmonicMean(List<Team> teams)
        {
            double sumOfReciprocals = 0;
            int n = teams.Count * 2;

            foreach (var team in teams)
            {
                int teamLeadSatisfaction = team.TeamLeadPriority;
                int juniorSatisfaction = team.JuniorPriority;

                sumOfReciprocals += (1.0 / teamLeadSatisfaction) + (1.0 / juniorSatisfaction);
            }

            return n / sumOfReciprocals;
        }
    }
}