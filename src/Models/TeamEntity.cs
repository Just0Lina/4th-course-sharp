namespace Nsu.HackathonProblem.Models;

public class TeamEntity
{
    public int Id { get; set; }
    public int HackathonId { get; set; }
    public int TeamLeadId { get; set; }
    public int JuniorId { get; set; }
    public virtual HackathonEntity Hackathon { get; set; }
}