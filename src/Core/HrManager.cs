using DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;
using Nsu.HackathonProblem.Models;

namespace Nsu.HackathonProblem.Core;

public class HrManager(
    ITeamFormationService teamBuildingStrategy,
    Hackathon hackathon)
{
    public List<Team> FormTeams(List<Employee> juniors,
        List<Employee> teamLeads)
    {
        var (juniorPreferences, teamLeadPreferences) =
            hackathon.GeneratePreferences(juniors, teamLeads);
        return teamBuildingStrategy.FormTeams(juniorPreferences,
            teamLeadPreferences);
    }
}