
namespace Nsu.HackathonProblem.Contracts.Models
{
    public class JuniorPreferences
    {
        public Employee Junior { get; set; }
        public Dictionary<Employee, int> PreferredTeamLeads { get; set; }
    }

    public class TeamLeadPreferences
    {
        public Employee TeamLead { get; set; }
        public Dictionary<Employee, int> PreferredJuniors { get; set; }

    }
}
