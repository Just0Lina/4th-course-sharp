using DreamTeamApp.Nsu.HackathonProblem.Services;
using Nsu.HackathonProblem.Core;

namespace Nsu.HackathonProblem.Workers
{
    public class HackathonWorker(
        HrDirector hrDirector,
        EmployeeService employeeService)
        : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await employeeService.SaveEmployeesFromCsvAsync(
                CsvFilePaths.JuniorsCsvPath, CsvFilePaths.TeamLeadsCsvPath);
            var juniors = await employeeService.GetJuniorsAsync();
            var teamLeads = await employeeService.GetTeamLeadsAsync();

            await hrDirector.OverseeHackathons(10, juniors, teamLeads);
            await hrDirector.PrintHackathonResultsAsync(6);
            await hrDirector.PrintAverageHarmonyAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}