namespace Nsu.HackathonProblem.SharedData.Models;

public class TeamsAndPreferencesEntity(
    List<Team> teams, int HackathonId)
{
    public List<Team> Teams { get; init; } = teams;
    public int HackathonId { get; set; } = HackathonId;
}