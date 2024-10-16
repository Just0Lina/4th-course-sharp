namespace Nsu.HackathonProblem.SharedData.Models;
public class TeamsAndPreferencesEntity(
   List<Wishlist> juniorPreferences,
   List<Wishlist> teamLeadPreferences,
   List<Team> teams)
{
   public List<Wishlist> JuniorPreferences { get; init; } = juniorPreferences;
   public List<Wishlist> TeamLeadPreferences { get; init; } = teamLeadPreferences;
   public List<Team> Teams { get; init; } = teams;
}