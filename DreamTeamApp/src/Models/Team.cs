namespace Nsu.HackathonProblem.Models;

public class Team
{
    public Team(Employee teamLead, Employee junior, int teamLeadPriority,
        int juniorPriority)
    {
        TeamLead = teamLead;
        Junior = junior;
        TeamLeadPriority = teamLeadPriority;
        JuniorPriority = juniorPriority;
    }

    public int Id { get; set; }

    public Employee TeamLead { get; set; }
    public Employee Junior { get; set; }
    public int TeamLeadPriority { get; set; }
    public int JuniorPriority { get; set; }
}