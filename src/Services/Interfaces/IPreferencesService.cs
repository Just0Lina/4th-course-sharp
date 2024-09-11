using Nsu.HackathonProblem.Models;

namespace DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;

public interface IPreferencesService
{
    List<EmployeePreferences> CreatePreferences(List<Employee> juniors,
        List<Employee> teamLeads);

    (List<EmployeePreferences>, List<EmployeePreferences>)
        GeneratePreferences(List<Employee> juniors,
            List<Employee> teamLeads);
}