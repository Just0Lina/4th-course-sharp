
namespace Nsu.HackathonProblem.Contracts.Services
{
    using System.Linq;
    using Nsu.HackathonProblem.Contracts.Models;


    public class PreferencesService : IPreferencesService
    {
        public List<JuniorPreferences> CreateJuniorPreferences(List<Employee> juniors, List<Employee> teamLeads)
        {
            var juniorPreferences = new List<JuniorPreferences>();

            foreach (var junior in juniors)
            {
                var preferences = teamLeads.OrderBy(x => Guid.NewGuid()).ToList();
                var preferredTeamLeads = preferences.Select((teamLead, index) => new { teamLead, priority = teamLeads.Count - index })
                                                     .ToDictionary(x => x.teamLead, x => x.priority);

                juniorPreferences.Add(new JuniorPreferences
                {
                    Junior = junior,
                    PreferredTeamLeads = preferredTeamLeads
                });
            }

            return juniorPreferences;
        }

        public List<TeamLeadPreferences> CreateTeamLeadPreferences(List<Employee> teamLeads, List<Employee> juniors)
        {
            var teamLeadPreferences = new List<TeamLeadPreferences>();

            foreach (var teamLead in teamLeads)
            {
                var preferences = juniors.OrderBy(x => Guid.NewGuid()).ToList();
                var preferredJuniors = preferences.Select((junior, index) => new { junior, priority = teamLeads.Count - index })
                                                     .ToDictionary(x => x.junior, x => x.priority);

                teamLeadPreferences.Add(new TeamLeadPreferences
                {
                    TeamLead = teamLead,
                    PreferredJuniors = preferredJuniors
                });
            }

            return teamLeadPreferences;
        }
    }
}
