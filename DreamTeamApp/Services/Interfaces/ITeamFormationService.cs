using Nsu.HackathonProblem.Contracts.Models;

namespace Nsu.HackathonProblem.Contracts.Services
{
    public interface ITeamFormationService
    {
        List<Team> FormTeams(List<EmployeePreferences> juniorPreferences, List<EmployeePreferences> teamLeadPreferences);
    }
}
