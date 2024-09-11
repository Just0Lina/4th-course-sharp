using DreamTeamApp.Nsu.HackathonProblem.Models;

namespace DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces
{
    public interface ITeamFormationService
    {
        List<Team> FormTeams(List<EmployeePreferences> juniorPreferences, List<EmployeePreferences> teamLeadPreferences);
    }
}
