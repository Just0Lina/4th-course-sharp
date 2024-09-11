using DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;
using Nsu.HackathonProblem.Models;

namespace DreamTeamApp.Nsu.HackathonProblem.Services;

public class PreferencesService : IPreferencesService
{
    public List<EmployeePreferences> CreatePreferences(
        List<Employee> employees, List<Employee> employeesForPreferences)
    {
        var workerPreferences = new List<EmployeePreferences>();

        foreach (var employee in employees)
        {
            var preferences = employeesForPreferences
                .OrderBy(x => Guid.NewGuid()).ToList();
            var preferredWorkers = preferences.Select((teamLead, index) =>
                    new
                    {
                        teamLead,
                        priority = employeesForPreferences.Count - index
                    })
                .ToDictionary(x => x.teamLead, x => x.priority);

            workerPreferences.Add(new EmployeePreferences(
                employee, preferredWorkers));
        }

        return workerPreferences;
    }

    public (List<EmployeePreferences>, List<EmployeePreferences>)
        GeneratePreferences(List<Employee> juniors, List<Employee> teamLeads)
    {
        var juniorPreferences = CreatePreferences(juniors, teamLeads);
        var teamLeadPreferences = CreatePreferences(teamLeads, juniors);
        return (juniorPreferences, teamLeadPreferences);
    }
}