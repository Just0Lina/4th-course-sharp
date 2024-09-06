
namespace Nsu.HackathonProblem.Contracts.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Nsu.HackathonProblem.Contracts.Models;

    public class TeamFormationService : ITeamFormationService
    {
        private readonly RatingService _ratingService;

        public TeamFormationService()
        {
            _ratingService = new RatingService();
        }

        public double CalculateOverallHarmony(List<Team> teams)
        {
            return _ratingService.CalculateHarmonicMean(teams);
        }
        public List<Team> FormTeams(List<JuniorPreferences> juniorPreferences, List<TeamLeadPreferences> teamLeadPreferences)
        {
            var teams = new List<Team>();

            var freeJuniors = new HashSet<JuniorPreferences>(juniorPreferences);
            var teamLeadMatches = new Dictionary<Employee, Employee>();
            var juniorProposals = juniorPreferences.ToDictionary(jp => jp.Junior, jp => new Queue<Employee>(jp.PreferredTeamLeads.Keys)); // Очередь предпочтений для каждого джуна

            while (freeJuniors.Count > 0)
            {
                var juniorPref = freeJuniors.First();
                freeJuniors.Remove(juniorPref);

                if (juniorProposals[juniorPref.Junior].Count > 0)
                {
                    var teamLead = juniorProposals[juniorPref.Junior].Dequeue();

                    if (!teamLeadMatches.ContainsKey(teamLead))
                    {
                        teamLeadMatches[teamLead] = juniorPref.Junior;
                    }
                    else
                    {
                        var currentJunior = teamLeadMatches[teamLead];
                        var teamLeadPref = teamLeadPreferences.First(tlp => tlp.TeamLead == teamLead);

                        if (teamLeadPref.PreferredJuniors[currentJunior] > teamLeadPref.PreferredJuniors[juniorPref.Junior])
                        {
                            freeJuniors.Add(juniorPref);
                        }
                        else
                        {
                            freeJuniors.Add(juniorPreferences.First(jp => jp.Junior == currentJunior));
                            teamLeadMatches[teamLead] = juniorPref.Junior;
                        }
                    }
                }
            }

            foreach (var match in teamLeadMatches)
            {
                var teamLead = match.Key;
                var junior = match.Value;

                var juniorPref = juniorPreferences.First(jp => jp.Junior == junior);
                var teamLeadPref = teamLeadPreferences.First(tlp => tlp.TeamLead == teamLead);

                int teamLeadPriority = teamLeadPref.PreferredJuniors[junior];
                int juniorPriority = juniorPref.PreferredTeamLeads[teamLead];

                teams.Add(new Team(teamLead, junior)
                {
                    TeamLeadPriority = teamLeadPriority,
                    JuniorPriority = juniorPriority
                });
            }

            return teams;
        }

    }
}
