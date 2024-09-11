using Microsoft.Extensions.Hosting;
using Nsu.HackathonProblem.Contracts.Models;
using Nsu.HackathonProblem.Contracts.Services;

namespace Nsu.HackathonProblem.Core
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