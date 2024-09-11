using DreamTeamApp.Nsu.HackathonProblem.Core;
using DreamTeamApp.Nsu.HackathonProblem.Models;
using DreamTeamApp.Nsu.HackathonProblem.Services;
using Microsoft.Extensions.Hosting;

namespace DreamTeamApp.Nsu.HackathonProblem.Workers
{
    public class HackathonWorker(
        HrDirector hrDirector)
        : IHostedService
    {
        private readonly List<Employee> _juniors = CsvReaderService.ReadEmployees("Resources/Juniors20.csv");
        private readonly List<Employee> _teamLeads = CsvReaderService.ReadEmployees("Resources/Teamleads20.csv");

        public Task StartAsync(CancellationToken cancellationToken)
        {
            hrDirector.OverseeHackathons(1000, _juniors, _teamLeads);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    
    
}