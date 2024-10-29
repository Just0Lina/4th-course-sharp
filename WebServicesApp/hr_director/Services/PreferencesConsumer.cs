using System.Text.Json;
using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;

namespace Nsu.HackathonProblem.HrDirector.Services;

public class PreferencesConsumer(
    ILogger<PreferencesConsumer> logger,
    IRabbitMqService rabbitMqService
)
    : BackgroundService
{
    
    public static event Action<PreferencesMessage> PreferencesReceivedTcs;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = $"preferences.submit.{Guid.NewGuid()}";

        rabbitMqService.Consume<PreferencesMessage>(queueName,
            "preferences.submit", message =>
            {
                var preferencesGetStartedEvent = new PreferencesMessage
                {
                    Employee = message.Employee,
                    Preferences = message.Preferences,
                    EmployeeType = message.EmployeeType,
                    HackathonId = message.HackathonId,
                };
                Console.WriteLine(
                    $"Sending preferences: {JsonSerializer.Serialize(preferencesGetStartedEvent)}");
                PreferencesReceivedTcs?.Invoke(preferencesGetStartedEvent);
                return Task.CompletedTask;
            }, stoppingToken);
        return Task.CompletedTask;
    }
}