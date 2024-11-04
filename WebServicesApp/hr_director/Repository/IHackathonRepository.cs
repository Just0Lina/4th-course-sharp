using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Repository;

public interface IHackathonRepository
{

    Task SaveEmployeesAsync(List<Team> teams);
    Task SaveHackathonIdAsync(int hackathonId);
    Task UpdateHackathonAsync(decimal harmonyIndex,
        List<TeamEntity> teamEntities, int hackathonId);

    Task<HackathonEntity>? GetHackathonByIdAsync(int id);
    Task<IEnumerable<Wishlist>> GetTeamLeadWishlistsAsync(int hackathonId);
    Task<IEnumerable<Wishlist>> GetJuniorWishlistsAsync(int hackathonId);
    Task<int> GetPreferencesCountAsync(int hackathonId, Role junior);
    Task ClearPreferencesAndTeamsForHackathonIdAsync(int requestHackathonId);
    Task SaveJuniorPreferences(int hackathonId, Wishlist messagePreferences);

    Task SaveTeamLeadPreferences(int hackathonId, Wishlist messagePreferences);
    bool AllRequestsReceived();
}
