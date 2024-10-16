using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrManager.Services
{
    public class TeamBuildingStrategy : ITeamBuildingStrategy
    {
        public IEnumerable<Team> BuildTeams(
            IEnumerable<Employee> teamLeads,
            IEnumerable<Employee> juniors,
            IEnumerable<Wishlist> teamLeadsWishlists,
            IEnumerable<Wishlist> juniorsWishlists)
        {
            var freeJuniors = new HashSet<Wishlist>(juniorsWishlists);
            var teamLeadMatches = new Dictionary<int, int>();
            var juniorProposals = juniorsWishlists.ToDictionary(jp => jp.EmployeeId, jp => new Queue<int>(jp.DesiredEmployees));

            while (freeJuniors.Count > 0)
            {
                var currentJuniorWishlist = freeJuniors.First();
                freeJuniors.Remove(currentJuniorWishlist);

                if (!juniorProposals[currentJuniorWishlist.EmployeeId].Any())
                    continue;

                var preferredTeamLeadId = juniorProposals[currentJuniorWishlist.EmployeeId].Dequeue();

                if (!teamLeadMatches.TryGetValue(preferredTeamLeadId, out var currentJuniorId))
                {
                    teamLeadMatches[preferredTeamLeadId] = currentJuniorWishlist.EmployeeId;
                }
                else
                {
                    HandleMatch(
                        teamLeadsWishlists,
                        preferredTeamLeadId,
                        currentJuniorWishlist,
                        currentJuniorId,
                        freeJuniors,
                        juniorsWishlists,
                        teamLeadMatches);
                }
            }

            return CreateTeams(teamLeads, juniors, teamLeadMatches);
        }

        private void HandleMatch(
            IEnumerable<Wishlist> teamLeadsWishlists,
            int preferredTeamLeadId,
            Wishlist currentJuniorWishlist,
            int currentJuniorId,
            HashSet<Wishlist> freeJuniors,
            IEnumerable<Wishlist> juniorsWishlists,
            Dictionary<int, int> teamLeadMatches)
        {
            var teamLeadWishlist = teamLeadsWishlists.First(tlp => tlp.EmployeeId == preferredTeamLeadId);

            var currentJuniorIndex = Array.IndexOf(teamLeadWishlist.DesiredEmployees, currentJuniorId);
            var newJuniorIndex = Array.IndexOf(teamLeadWishlist.DesiredEmployees, currentJuniorWishlist.EmployeeId);

            if (newJuniorIndex < currentJuniorIndex)
            {
                freeJuniors.Add(juniorsWishlists.First(jp => jp.EmployeeId == currentJuniorId));
                teamLeadMatches[preferredTeamLeadId] = currentJuniorWishlist.EmployeeId;
            }
            else
            {
                freeJuniors.Add(currentJuniorWishlist);
            }
        }

        private IEnumerable<Team> CreateTeams(IEnumerable<Employee> teamLeads, IEnumerable<Employee> juniors, Dictionary<int, int> teamLeadMatches)
        {
            var teamLeadDictionary = teamLeads.ToDictionary(tl => tl.Id);
            var juniorDictionary = juniors.ToDictionary(j => j.Id);

            return teamLeadMatches.Select(match => new Team(
                teamLeadDictionary[match.Key],
                juniorDictionary[match.Value]));
        }
    }
}
