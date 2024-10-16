using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrManager.Services;

public interface ITeamBuildingStrategy
{
    IEnumerable<Team> BuildTeams(
        IEnumerable<Employee> teamLeads,
        IEnumerable<Employee> juniors,
        IEnumerable<Wishlist> teamLeadsWishlists,
        IEnumerable<Wishlist> juniorsWishlists);
}