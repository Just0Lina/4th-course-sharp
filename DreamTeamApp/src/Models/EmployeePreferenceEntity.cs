namespace Nsu.HackathonProblem.Models;

public class EmployeePreferenceEntity
{
    public int HackathonId { get; set; }
    public Role Role { get; set; }
    public int EmployeeId { get; set; }
    public int PreferredEmployeeId { get; set; }
    public int Priority { get; set; }
}