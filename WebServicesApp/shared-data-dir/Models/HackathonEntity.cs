namespace Nsu.HackathonProblem.SharedData.Models;

public class HackathonEntity
{
    public int Id { get; set; }
    public decimal Harmony { get; set; }
    public List<TeamEntity> Teams { get; set; }
}