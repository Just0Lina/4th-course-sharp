using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.Database;
using Nsu.HackathonProblem.Models;

namespace DreamTeamApp.Nsu.HackathonProblem.Services;

public class EmployeeService(HackathonDbContext context)
{
    public async Task SaveEmployeesFromCsvAsync(string juniorsCsvPath,
        string teamLeadsCsvPath)
    {
        var juniors = CsvReaderService.ReadEmployees(juniorsCsvPath);
        var teamLeads = CsvReaderService.ReadEmployees(teamLeadsCsvPath);

        await SaveJuniorsAsync(juniors);
        await SaveTeamLeadsAsync(teamLeads);
    }


    private async Task SaveJuniorsAsync(List<Employee> juniors)
    {
        foreach (var junior in juniors)
        {
            var existingJunior = await context.Employees
                .FirstOrDefaultAsync(e =>
                    e.Id == junior.Id && e.Role == Role.Junior);

            if (existingJunior == null)
            {
                var newJunior =
                    new EmployeeEntity(junior.Id, junior.Name, Role.Junior);

                context.Employees.Add(newJunior);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SaveTeamLeadsAsync(List<Employee> teamLeads)
    {
        foreach (var teamLead in teamLeads)
        {
            var existingTeamLead = await context.Employees
                .FirstOrDefaultAsync(e =>
                    e.Id == teamLead.Id && e.Role == Role.TeamLead);

            if (existingTeamLead == null)
            {
                var newTeamLead =
                    new EmployeeEntity(teamLead.Id, teamLead.Name,
                        Role.TeamLead);

                context.Employees.Add(newTeamLead);
            }
        }

        await context.SaveChangesAsync();
    }


    public async Task<List<Employee>> GetJuniorsAsync()
    {
        var juniors = await context.Employees
            .Where(e => e.Role == Role.Junior)
            .Select(j => new Employee(j.Id, j.Name))
            .ToListAsync();

        return juniors;
    }

    public async Task<List<Employee>> GetTeamLeadsAsync()
    {
        var teamLeads = await context.Employees
            .Where(e => e.Role == Role.TeamLead)
            .Select(t => new Employee(t.Id, t.Name))
            .ToListAsync();

        return teamLeads;
    }
}