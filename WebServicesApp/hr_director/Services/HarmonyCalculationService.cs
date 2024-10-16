using Nsu.HackathonProblem.HrDirector.Repository;
using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Services;

public class HarmonyCalculationService(  IHackathonRepository hackathonRepository) : IHarmonyCalculationService
{
    public double CalculateHarmony(List<Wishlist> juniorPreferences,
        List<Wishlist> teamLeadPreferences, List<Team> teams)
    {
        var n = teams.Count * 2;

        double sumOfReciprocals = 0;

        foreach (var team in teams)
        {
            var teamLeadPreference = teamLeadPreferences.FirstOrDefault(tlp => tlp.EmployeeId == team.TeamLead.Id);
            var teamLeadSatisfactionIndex = teamLeadPreference != null ? Array.IndexOf(teamLeadPreference.DesiredEmployees, team.Junior.Id) + 1 : -1;

            var juniorPreference = juniorPreferences.FirstOrDefault(jp => jp.EmployeeId == team.Junior.Id);
            var juniorSatisfactionIndex = juniorPreference != null ? Array.IndexOf(juniorPreference.DesiredEmployees, team.TeamLead.Id) + 1 : -1;
            sumOfReciprocals += (1.0 / teamLeadSatisfactionIndex) + (1.0 / juniorSatisfactionIndex);
        }

        return n / sumOfReciprocals;
    }
    

    public async Task SaveHackathon(List<Wishlist> juniorPreferences,
        List<Wishlist> teamLeadPreferences, List<Team> teams,
        double harmonyIndex)
    {
        var hackathon = new HackathonEntity
        {
            Harmony = (decimal)harmonyIndex,
            Teams = teams.Select(t => new TeamEntity
            {
                TeamLeadId = t.TeamLead.Id,
                JuniorId = t.Junior.Id
            }).ToList()
        };
        await hackathonRepository.AddHackathonAsync(hackathon);
        await hackathonRepository.SavePreferencesAsync(juniorPreferences,
            Role.Junior, hackathon.Id);
        await hackathonRepository.SavePreferencesAsync(teamLeadPreferences,
            Role.TeamLead, hackathon.Id);
        await hackathonRepository.SaveEmployeesAsync(teams);
    }
}