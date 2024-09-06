namespace Nsu.HackathonProblem.Core
{
    using Nsu.HackathonProblem.Contracts.Models;

    public class HrDirector
    {
        private readonly HrManager _hrManager;

        public HrDirector(HrManager hrManager)
        {
            _hrManager = hrManager;
        }

        public void OverseeHackathons(int hackathonCount, List<Employee> juniors, List<Employee> teamLeads)
        {
            double totalHarmony = 0;

            for (int i = 1; i <= hackathonCount; i++)
            {
                Console.WriteLine($"--- Hackathon #{i} ---");

                // Form teams and calculate harmony
                var teams = _hrManager.FormTeams(juniors, teamLeads);
                double harmony = CalculateOverallHarmony(teams);

                totalHarmony += harmony;
                Console.WriteLine($"Hackathon Harmony: {harmony:F2}");
            }

            double averageHarmony = totalHarmony / hackathonCount;
            Console.WriteLine($"Average Harmony after {hackathonCount} Hackathons: {averageHarmony:F2}");
        }

        public double CalculateOverallHarmony(List<Team> teams)
        {
            double sum = teams.Sum(team =>
                2.0 / ((1.0 / team.TeamLeadPriority) + (1.0 / team.JuniorPriority)));

            return sum / teams.Count;
        }
    }
}