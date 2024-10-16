using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Services;

public interface IHarmonyCalculationService
{
    double CalculateHarmony(List<Wishlist> juniorPreferences,
        List<Wishlist> teamLeadPreferences, List<Team> teams);

    Task SaveHackathon(List<Wishlist> juniorPreferences,
        List<Wishlist> teamLeadPreferences, List<Team> teams,
        double harmonyIndex);
}