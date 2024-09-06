using Nsu.HackathonProblem.Contracts.Services;
namespace Nsu.HackathonProblem.Core
{
    using Nsu.HackathonProblem.Contracts.Models;

    public class Hackathon
    {
        private readonly IPreferencesService _preferencesService;

        public Hackathon(IPreferencesService preferencesService)
        {
            _preferencesService = preferencesService;
        }

        public (List<JuniorPreferences>, List<TeamLeadPreferences>) GeneratePreferences(List<Employee> juniors, List<Employee> teamLeads)
        {
            var juniorPreferences = _preferencesService.CreateJuniorPreferences(juniors, teamLeads);
            var teamLeadPreferences = _preferencesService.CreateTeamLeadPreferences(teamLeads, juniors);
            return (juniorPreferences, teamLeadPreferences);
        }
    }
}