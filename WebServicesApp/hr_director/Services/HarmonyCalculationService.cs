using Nsu.HackathonProblem.HrDirector.Repository;
using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Services;

public class HarmonyCalculationService(
    IHackathonRepository hackathonRepository,
    IServiceScopeFactory contextFactory,
    ILogger<HarmonyCalculationService> logger)
    : IHarmonyCalculationService
{

    private async Task<bool> CheckPreferencesCountAsync(int hackathonId)
    {
        using var scope = contextFactory.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IHackathonRepository>();

        var juniorCount =
            await repository.GetPreferencesCountAsync(hackathonId,
                Role.Junior);

        var teamLeadCount =
            await repository.GetPreferencesCountAsync(hackathonId,
                Role.TeamLead);
        logger.LogDebug($"junior count: {juniorCount}, team lead count: {teamLeadCount}, hackathonId: {hackathonId}");

        return juniorCount >= 25 && teamLeadCount >= 25;
    }


    public async Task<double> CalculateHarmonyAsync(List<Team> teams,
        int hackathonId)
    {

        while (!await CheckPreferencesCountAsync(hackathonId))
        {
        }
        
        var juniorWishlistsAsync =
            await hackathonRepository.GetJuniorWishlistsAsync(hackathonId);

        var teamLeadsWishlistsAsync = await hackathonRepository
            .GetTeamLeadWishlistsAsync(hackathonId);

        return CalculateHarmony(juniorWishlistsAsync.ToList(),
            teamLeadsWishlistsAsync.ToList(),
            teams);
    }


    private double CalculateHarmony(List<Wishlist> juniorPreferences,
        List<Wishlist> teamLeadPreferences, List<Team> teams)
    {
        var n = teams.Count * 2;

        double sumOfReciprocals = 0;
        foreach (var team in teams)
        {
            var teamLeadPreference =
                teamLeadPreferences.FirstOrDefault(tlp =>
                    tlp.EmployeeId == team.TeamLead.Id);
            var teamLeadSatisfactionIndex = teamLeadPreference != null
                ? Array.IndexOf(teamLeadPreference.DesiredEmployees,
                    team.Junior.Id) + 1
                : -1;

            var juniorPreference =
                juniorPreferences.FirstOrDefault(jp =>
                    jp.EmployeeId == team.Junior.Id);
            var juniorSatisfactionIndex = juniorPreference != null
                ? Array.IndexOf(juniorPreference.DesiredEmployees,
                    team.TeamLead.Id) + 1
                : -1;
            sumOfReciprocals += (1.0 / teamLeadSatisfactionIndex) +
                                (1.0 / juniorSatisfactionIndex);
        }

        return n / sumOfReciprocals;
    }


    public async Task SaveHackathon(List<Team> teams,
        double harmonyIndex, int hackathonId)
    {
        var teamEntities = teams.Select(t => new TeamEntity
        {
            TeamLeadId = t.TeamLead.Id,
            JuniorId = t.Junior.Id
        }).ToList();
        await hackathonRepository.UpdateHackathonAsync(
            (decimal)harmonyIndex,
            teamEntities, hackathonId);

        await hackathonRepository.SaveEmployeesAsync(teams);
    }
}