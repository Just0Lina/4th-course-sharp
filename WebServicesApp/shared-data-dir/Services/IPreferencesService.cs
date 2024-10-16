using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.SharedData.Services;

public interface IPreferencesService
{
    Wishlist CreatePreferences(Employee junior,
        List<Employee> teamLeads);
    
}