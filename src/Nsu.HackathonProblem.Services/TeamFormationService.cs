using DreamTeamApp.Nsu.HackathonProblem.Models;
using DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;

namespace DreamTeamApp.Nsu.HackathonProblem.Services;

public class TeamFormationService : ITeamFormationService
{
    public List<Team> FormTeams(List<EmployeePreferences> juniorPreferences,
        List<EmployeePreferences> teamLeadPreferences)
    {
        var freeJuniors =
            new HashSet<EmployeePreferences>(juniorPreferences);
        var teamLeadMatches = new Dictionary<Employee, Employee>();
        var juniorProposals = juniorPreferences.ToDictionary(
            jp => jp.Employee,
            jp => new Queue<Employee>(jp.PreferredEmployees
                .Keys)); // Очередь предпочтений для каждого джуна

        while (freeJuniors.Count > 0)
        {
            var juniorPref = freeJuniors.First();
            freeJuniors.Remove(juniorPref);

            if (juniorProposals[juniorPref.Employee].Count <= 0) continue;
            var teamLead = juniorProposals[juniorPref.Employee].Dequeue();

            if (!teamLeadMatches.TryGetValue(teamLead, out var value))
            {
                teamLeadMatches[teamLead] = juniorPref.Employee;
            }
            else
            {
                var currentJunior = value;
                var teamLeadPref =
                    teamLeadPreferences.First(tlp =>
                        tlp.Employee == teamLead);

                if (teamLeadPref.PreferredEmployees[currentJunior] >
                    teamLeadPref.PreferredEmployees[juniorPref.Employee])
                {
                    freeJuniors.Add(juniorPref);
                }
                else
                {
                    freeJuniors.Add(juniorPreferences.First(jp =>
                        jp.Employee == currentJunior));
                    teamLeadMatches[teamLead] = juniorPref.Employee;
                }
            }
        }

        return (from match in teamLeadMatches
            let teamLead = match.Key
            let junior = match.Value
            let juniorPref =
                juniorPreferences.First(jp => jp.Employee == junior)
            let teamLeadPref =
                teamLeadPreferences.First(tlp => tlp.Employee == teamLead)
            let teamLeadPriority = teamLeadPref.PreferredEmployees[junior]
            let juniorPriority = juniorPref.PreferredEmployees[teamLead]
            select new Team(teamLead, junior, teamLeadPriority,
                juniorPriority)).ToList();
    }
}