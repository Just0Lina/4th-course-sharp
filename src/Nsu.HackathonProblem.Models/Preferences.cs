
namespace DreamTeamApp.Nsu.HackathonProblem.Models
{
    public class EmployeePreferences(
        Employee employee,
        Dictionary<Employee, int> preferredEmployees)
    {
        public Employee Employee { get; init; } = employee;
        public Dictionary<Employee, int> PreferredEmployees { get; init; } = preferredEmployees;
    }
}
