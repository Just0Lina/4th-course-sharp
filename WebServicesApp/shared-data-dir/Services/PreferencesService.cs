using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.SharedData.Services;

public class PreferencesService
    : IPreferencesService
{
    public Wishlist CreatePreferences(
        Employee employee, List<Employee> employeesForPreferences)
    {
        var preferences = employeesForPreferences
            .OrderBy(x => Guid.NewGuid()).ToList();
        var preferredWorkers = preferences.Select((teamLead, index) =>
                teamLead.Id
            )
            .ToArray();

        return new Wishlist(
            employee.Id, preferredWorkers);
    }
    
}
