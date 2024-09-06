using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
namespace Nsu.HackathonProblem.Core
{
    using Nsu.HackathonProblem.Contracts.Models;
    using Nsu.HackathonProblem.Contracts.Services;


    public class HackathonWorker : IHostedService
    {
        private readonly HrDirector _hrDirector;
        private readonly List<Employee> _juniors;
        private readonly List<Employee> _teamLeads;

        public HackathonWorker(HrDirector hrDirector, CsvReaderService csvReaderService)
        {
            _hrDirector = hrDirector;
            _juniors = csvReaderService.ReadEmployees("Resources/Juniors20.csv");
            _teamLeads = csvReaderService.ReadEmployees("Resources/Teamleads20.csv");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _hrDirector.OverseeHackathons(1000, _juniors, _teamLeads);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}