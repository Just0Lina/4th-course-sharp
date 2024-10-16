using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;

namespace Nsu.HackathonProblem.TeamLead.Service;

public class StartupService(
    IHttpClientFactory httpClientFactory,
    IPreferencesService preferencesService,
    ILogger<StartupService> logger)
    : IHostedService
{
    private const string EmployeeIdEnvVariable = "EMPLOYEE_ID";
    private const string EmployeeTypeEnvVariable = "EMPLOYEE_TYPE";

    private const string SubmitPreferencesUrlTemplate =
        "http://hr_manager:8080/api/hr/submit-{0}-preferences";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var employeeType = GetEmployeeTypeFromEnvironment();
        Console.WriteLine(employeeType);

        var employeeId = GetEmployeeIdFromEnvironment();

        if (employeeId == null || string.IsNullOrEmpty(employeeType))
        {
            logger.LogWarning("Invalid environment variables. ");
            return;
        }

        var employee = await GetEmployeeAsync(employeeId.Value, employeeType);
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

    private string? GetEmployeeTypeFromEnvironment()
    {
        return Environment.GetEnvironmentVariable(EmployeeTypeEnvVariable);
    }

    private int? GetEmployeeIdFromEnvironment()
    {
        var employeeIdStr =
            Environment.GetEnvironmentVariable(EmployeeIdEnvVariable);
        if (int.TryParse(employeeIdStr, out var employeeId))
        {
            return employeeId;
        }

        return null;
    }

    private async Task<Employee?> GetEmployeeAsync(int employeeId,
        string employeeType)
    {
        if (employeeType == "teamlead")
        {
            return DataService.ReadEmployeeById(DataService.JuniorsCsv,
                employeeId);
        }

        return DataService.ReadEmployeeById(DataService.teamLeadsCsv,
            employeeId);
    }

    private async Task<List<Employee>> GetTeamLeadsAsync()
    {
        return DataService.ReadEmployees(DataService.teamLeadsCsv);
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