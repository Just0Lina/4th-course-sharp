namespace Nsu.HackathonProblem.Models;

public class Team(
    Employee teamLead,
    Employee junior,
    int teamLeadPriority,
    int juniorPriority)
{
    public Employee TeamLead { get; } = teamLead;
    public Employee Junior { get; } = junior;
    public int TeamLeadPriority { get; } = teamLeadPriority;
    public int JuniorPriority { get; } = juniorPriority;
}