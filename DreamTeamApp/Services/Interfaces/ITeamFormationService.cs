
using Nsu.HackathonProblem.Contracts.Models;

namespace Nsu.HackathonProblem.Contracts.Services
{
    public interface ITeamFormationService
    {
        List<Team> FormTeams(List<JuniorPreferences> juniorPreferences, List<TeamLeadPreferences> teamLeadPreferences);
        double CalculateOverallHarmony(List<Team> teams);
    }
}
