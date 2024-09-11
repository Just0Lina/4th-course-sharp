using Nsu.HackathonProblem.Contracts.Models;
using Nsu.HackathonProblem.Contracts.Services;

namespace Nsu.HackathonProblem.Core
{
    public class HrManager(
        ITeamFormationService teamBuildingStrategy,
        Hackathon hackathon)
    {
        public List<Team> FormTeams(List<Employee> juniors, List<Employee> teamLeads)
        {
            var (juniorPreferences, teamLeadPreferences) = hackathon.GeneratePreferences(juniors, teamLeads);
            return teamBuildingStrategy.FormTeams(juniorPreferences, teamLeadPreferences);
        }
    }
}