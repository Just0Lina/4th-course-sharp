using System.Text.Json;
using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;
using Nsu.HackathonProblem.TeamLead.Configurations;

namespace Nsu.HackathonProblem.TeamLead.Service;

public class StartupService(
    IPreferencesService preferencesService,
    ILogger<StartupService> logger,
    EmployeeSettings employeeSettings,
    IRabbitMqService rabbitMqService)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        HackathonStartConsumer.HackathonStarted += OnHackathonStarted;
        return Task.CompletedTask;
    }

    private const string SubmitPreferencesUrlTemplate =
        "http://hr_manager:8080/api/hr/submit-{0}-preferences";

    private async void OnHackathonStarted(
        HackathonAnnouncementMessage hackathonStartedEvent)
    {
        var cancellationToken = CancellationToken.None;

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
        var preferencesMessage = new PreferencesMessage
        {
            Employee = employee,
            Preferences = preferences,
            EmployeeType = employeeType
        };
        var preferencesJson = JsonSerializer.Serialize(preferencesMessage);
        logger.LogInformation($"Received message: {preferencesJson}");
        try
        {
            logger.LogInformation("Publishing preferences to RabbitMQ.");
            rabbitMqService.Publish("preferences.submit", preferencesMessage);
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to publish preferences: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        HackathonStartConsumer.HackathonStarted -= OnHackathonStarted;
        return Task.CompletedTask;
    }
}