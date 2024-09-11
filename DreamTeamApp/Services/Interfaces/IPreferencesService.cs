using Nsu.HackathonProblem.Contracts.Models;

namespace Nsu.HackathonProblem.Contracts.Services;

public interface IPreferencesService
{
    List<EmployeePreferences> CreatePreferences(List<Employee> juniors,
        List<Employee> teamLeads);

    (List<EmployeePreferences>, List<EmployeePreferences>)
        GeneratePreferences(List<Employee> juniors,
            List<Employee> teamLeads);
}