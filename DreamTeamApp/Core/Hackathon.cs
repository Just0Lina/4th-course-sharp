using Nsu.HackathonProblem.Contracts.Models;
using Nsu.HackathonProblem.Contracts.Services;

namespace Nsu.HackathonProblem.Core;

public class Hackathon(IPreferencesService preferencesService)
{
    public (List<EmployeePreferences>, List<EmployeePreferences>)
        GeneratePreferences(List<Employee> juniors, List<Employee> teamLeads)
    {
        return preferencesService.GeneratePreferences(juniors, teamLeads);
    }
}