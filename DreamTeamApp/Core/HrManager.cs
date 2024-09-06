namespace Nsu.HackathonProblem.Core
{
    using Nsu.HackathonProblem.Contracts.Services;
    using Nsu.HackathonProblem.Contracts.Models;
    public class HrManager
    {
        private readonly ITeamFormationService _teamBuildingStrategy;
        private readonly Hackathon _hackathon;

        public HrManager(ITeamFormationService teamBuildingStrategy, Hackathon hackathon)
        {
            _teamBuildingStrategy = teamBuildingStrategy;
            _hackathon = hackathon;
        }


        public List<Team> FormTeams(List<Employee> juniors, List<Employee> teamLeads)
        {
            var (juniorPreferences, teamLeadPreferences) = _hackathon.GeneratePreferences(juniors, teamLeads);

            // Use the strategy to build teams
            return _teamBuildingStrategy.FormTeams(juniorPreferences, teamLeadPreferences);
        }
    }
}