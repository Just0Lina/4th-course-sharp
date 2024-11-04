using System.Text.Json;
using MassTransit;
using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;
using Nsu.HackathonProblem.TeamLead.Configurations;

namespace Nsu.HackathonProblem.TeamLead.Service;

public class StartupService(
    IPreferencesService preferencesService,
    ILogger<StartupService> logger,
    EmployeeSettings employeeSettings,
    IServiceProvider serviceProvider,
    HackathonStartConsumer hackathonStartConsumer)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        HackathonStartedHandler.HackathonStarted += OnHackathonStarted;
        return Task.CompletedTask;
    }

    private bool _isProcessing = false;

    private async void OnHackathonStarted(
        HackathonAnnouncementMessage hackathonStartedEvent)
    {
        if (_isProcessing)
        {
            logger.LogInformation("Ignoring duplicate HackathonStarted event.");
            return;
        }
        
        _isProcessing = true;

        try
        {
            logger.LogInformation("HackathonStarted!");
            var cancellationToken = CancellationToken.None;
            var employeeType = employeeSettings.EmployeeType;
            var employeeId = employeeSettings.EmployeeId;

            if (string.IsNullOrEmpty(employeeType))
            {
                logger.LogWarning("Invalid environment variables.");
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
                preferencesService.CreatePreferences(employee,
                    employeePreferences);
            await SubmitPreferencesAsync(employee, preferences, employeeType,
                hackathonStartedEvent.HackathonId);
        }
        finally
        {        
            _isProcessing = false; 
        }
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
        Wishlist preferences, string employeeType, int hackathonId)
    {
        var preferencesMessage = new PreferencesMessage
        {
            Employee = employee,
            Preferences = preferences,
            EmployeeType = employeeType,
            HackathonId = hackathonId
        };

        var preferencesJson = JsonSerializer.Serialize(preferencesMessage);
        logger.LogInformation($"Received message: {preferencesJson}");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var publishEndpoint =
                scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            logger.LogInformation("Publishing preferences.");
            await publishEndpoint.Publish(preferencesMessage);
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to publish preferences: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        HackathonStartedHandler.HackathonStarted -= OnHackathonStarted;
        return Task.CompletedTask;
    }
}