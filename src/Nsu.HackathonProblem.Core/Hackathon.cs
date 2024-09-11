using DreamTeamApp.Nsu.HackathonProblem.Models;
using DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;

namespace DreamTeamApp.Nsu.HackathonProblem.Core;

public class Hackathon(IPreferencesService preferencesService)
{
    public (List<EmployeePreferences>, List<EmployeePreferences>)
        GeneratePreferences(List<Employee> juniors, List<Employee> teamLeads)
    {
        return preferencesService.GeneratePreferences(juniors, teamLeads);
    }
}