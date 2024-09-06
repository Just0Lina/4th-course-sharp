
namespace Nsu.HackathonProblem.Contracts.Models
{
    public class Team
    {
        public Employee TeamLead { get; set; }
        public Employee Junior { get; set; }

        public int TeamLeadPriority { get; set; }
        public int JuniorPriority { get; set; }

        public Team(Employee teamLead, Employee junior)
        {
            TeamLead = teamLead;
            Junior = junior;
        }
    }
}
