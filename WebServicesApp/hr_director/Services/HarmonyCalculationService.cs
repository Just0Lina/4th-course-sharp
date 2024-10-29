using Nsu.HackathonProblem.HrDirector.Repository;
using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Services;

public class HarmonyCalculationService : IHarmonyCalculationService
{
    private readonly IHackathonRepository _hackathonRepository;
    private readonly TaskCompletionSource<bool> _preferencesReceivedTcs;
    private readonly IServiceScopeFactory _scopeFactory;
    private int _hackathonId;


    public HarmonyCalculationService(IHackathonRepository hackathonRepository,
        IServiceScopeFactory contextFactory)
    {
        _hackathonRepository = hackathonRepository;
        _preferencesReceivedTcs = new TaskCompletionSource<bool>();
        _scopeFactory = contextFactory;


        PreferencesConsumer.PreferencesReceivedTcs += GetPreferencesConsumer;
    }


    private async void GetPreferencesConsumer(PreferencesMessage message)
    {
        _hackathonId = message.HackathonId;
        if (message.EmployeeType == "junior")
        {
            await _hackathonRepository.SavePreferenceAsync(
                Role.Junior, _hackathonId, message.Preferences);
        }
        else if (message.EmployeeType == "teamlead")
        {
            await _hackathonRepository.SavePreferenceAsync(
                Role.TeamLead, _hackathonId, message.Preferences);
        }

        bool allPreferencesReceived =
            await CheckPreferencesCountAsync(_hackathonId);

        if (allPreferencesReceived)
        {
            _preferencesReceivedTcs.TrySetResult(true);
        }
    }

    private async Task<bool> CheckPreferencesCountAsync(int hackathonId)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IHackathonRepository>();

        var juniorCount =
            await repository.GetPreferencesCountAsync(hackathonId,
                Role.Junior);

        var teamLeadCount =
            await repository.GetPreferencesCountAsync(hackathonId,
                Role.TeamLead);
        return juniorCount >= 25 && teamLeadCount >= 25;
    }


    public async Task<double> CalculateHarmonyAsync(List<Team> teams,
        CancellationToken cancellationToken)
    {
        while (!await CheckPreferencesCountAsync(_hackathonId))
        {
        }

        Console.WriteLine($"Calculating Harmony");

        var juniorWishlistsAsync =
            await _hackathonRepository.GetJuniorWishlistsAsync(_hackathonId);

        var teamLeadsWishlistsAsync = await _hackathonRepository
            .GetTeamLeadWishlistsAsync(_hackathonId);

        return CalculateHarmony(juniorWishlistsAsync.ToList(),
            teamLeadsWishlistsAsync.ToList(),
            teams);
    }


    private double CalculateHarmony(List<Wishlist> juniorPreferences,
        List<Wishlist> teamLeadPreferences, List<Team> teams)
    {
        var n = teams.Count * 2;

        double sumOfReciprocals = 0;
        Console.WriteLine($"Calculating harmy for {n} teams");
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
        await _hackathonRepository.UpdateHackathonAsync(
            (decimal)harmonyIndex,
            teamEntities, hackathonId);

        await _hackathonRepository.SaveEmployeesAsync(teams);
    }
}