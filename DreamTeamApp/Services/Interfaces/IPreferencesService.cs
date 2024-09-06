
using Nsu.HackathonProblem.Contracts.Models;

namespace Nsu.HackathonProblem.Contracts.Services
{
    public interface IPreferencesService
    {
        List<JuniorPreferences> CreateJuniorPreferences(List<Employee> juniors, List<Employee> teamLeads);
        List<TeamLeadPreferences> CreateTeamLeadPreferences(List<Employee> teamLeads, List<Employee> juniors);
    }
}
