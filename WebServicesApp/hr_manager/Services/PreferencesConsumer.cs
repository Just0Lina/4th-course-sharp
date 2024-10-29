using System.Text.Json;
using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;

namespace Nsu.HackathonProblem.HrManager.Services;

public class PreferencesConsumer(
    ILogger<PreferencesConsumer> logger,
    IDistributionService distributionService,
    IRabbitMqService rabbitMqService
)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = $"preferences.submit.{Guid.NewGuid()}";

        rabbitMqService.Consume<PreferencesMessage>(queueName,
            "preferences.submit",
            HandlePreferencesAsync, stoppingToken);
    }

    private async Task HandlePreferencesAsync(PreferencesMessage message)
    {
        distributionService.SetHackathonId(message.HackathonId);

        if (message.EmployeeType == "junior")
        {
            distributionService.SaveJuniorPreferences(
                new RequestToHr(message.Employee, message.Preferences));
        }
        else if (message.EmployeeType == "teamlead")
        {
            distributionService.SaveTeamLeadPreferences(
                new RequestToHr(message.Employee, message.Preferences));
        }

        logger.LogInformation(
            $"Received message: {JsonSerializer.Serialize(message)}");
    }
}