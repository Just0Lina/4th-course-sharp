using DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;
using Nsu.HackathonProblem.Models;

namespace Nsu.HackathonProblem.Core;

public class HrManager(
    ITeamFormationService teamBuildingStrategy,
    Hackathon hackathon)
{
    public (List<EmployeePreferences>, List<EmployeePreferences>)
        GetPreferences(List<Employee> juniors,
            List<Employee> teamLeads)
    {
        return
            hackathon.GeneratePreferences(juniors, teamLeads);
    }

    public List<Team> FormTeams(List<EmployeePreferences> juniorPreferences,
        List<EmployeePreferences> teamLeadPreferences)
    {
        return teamBuildingStrategy.FormTeams(juniorPreferences,
            teamLeadPreferences);
    }
}