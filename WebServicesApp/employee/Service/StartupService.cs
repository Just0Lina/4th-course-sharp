using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;
using Nsu.HackathonProblem.TeamLead.Configurations;

namespace Nsu.HackathonProblem.TeamLead.Service;

public class StartupService(
    IHttpClientFactory httpClientFactory,
    IPreferencesService preferencesService,
    ILogger<StartupService> logger,
    EmployeeSettings employeeSettings)
    : IHostedService
{
    private const string SubmitPreferencesUrlTemplate =
        "http://hr_manager:8080/api/hr/submit-{0}-preferences";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var employeeType = employeeSettings.EmployeeType;
        var employeeId = employeeSettings.EmployeeId;

        if (string.IsNullOrEmpty(employeeType))
        {
            logger.LogWarning("Invalid environment variables. ");
            return;
        }

        var employee = await GetEmployeeAsync(employeeId, employeeType);
        if (employee == null)
        {
            logger.LogWarning("Employee not found.");
            return;
        }

        var employeePreferences = await GetTeamLeadsAsync();
        var preferences =
            preferencesService.CreatePreferences(employee, employeePreferences);
        await SubmitPreferencesAsync(employee, preferences, employeeType,
            cancellationToken);
    }
    

    private async Task<Employee?> GetEmployeeAsync(int employeeId,
        string employeeType)
    {
        if (employeeType == "teamlead")
        {
            return DataService.ReadEmployeeById(DataService.JuniorsCsv,
                employeeId);
        }

        return DataService.ReadEmployeeById(DataService.TeamLeadsCsv,
            employeeId);
    }

    private async Task<List<Employee>> GetTeamLeadsAsync()
    {
        return DataService.ReadEmployees(DataService.TeamLeadsCsv);
    }

    private async Task SubmitPreferencesAsync(Employee employee,
        Wishlist preferences, string employeeType,
        CancellationToken cancellationToken)
    {
        var request = new RequestToHr(employee, preferences);
        var client = httpClientFactory.CreateClient();
        var submitUrl =
            string.Format(SubmitPreferencesUrlTemplate, employeeType);

        var response = await client.PostAsJsonAsync(
            submitUrl,
            request,
            cancellationToken: cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Preferences submitted successfully.");
        }
        else
        {
            logger.LogError(
                $"Failed to submit preferences. Status code: {response.StatusCode}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}