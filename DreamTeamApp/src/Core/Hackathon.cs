using DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;
using Nsu.HackathonProblem.Models;

namespace Nsu.HackathonProblem.Core;

public class Hackathon(IPreferencesService preferencesService)
{
    public (List<EmployeePreferences>, List<EmployeePreferences>)
        GeneratePreferences(List<Employee> juniors, List<Employee> teamLeads)
    {
        return preferencesService.GeneratePreferences(juniors, teamLeads);
    }
}