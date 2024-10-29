using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;

namespace Nsu.HackathonProblem.TeamLead.Service;

public class HackathonStartConsumer(
    IRabbitMqService rabbitMqService,
    ILogger<HackathonStartConsumer> logger)
    : BackgroundService
{
    public static event Action<HackathonAnnouncementMessage> HackathonStarted;
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var queueName = $"hackathon.start.{Guid.NewGuid()}";
        
        rabbitMqService.Consume<HackathonAnnouncementMessage>(queueName, "hackathon.start", async message =>
        {
            var hackathonStartedEvent = new HackathonAnnouncementMessage
            {
                HackathonId = message.HackathonId
            };

            logger.LogInformation($"Received hackathon start message: {message.Message}");
            HackathonStarted?.Invoke(hackathonStartedEvent);
            
            
        }, stoppingToken);

        return Task.CompletedTask;
    }
}
