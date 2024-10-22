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
        rabbitMqService.Consume<HackathonAnnouncementMessage>("hackathon.start", async message =>
        {
            var hackathonStartedEvent = new HackathonAnnouncementMessage
            {
                HackathonId = message.HackathonId
            };

            HackathonStarted?.Invoke(hackathonStartedEvent);

            logger.LogInformation($"Received hackathon start message: {message.Message}");
        }, stoppingToken);

        return Task.CompletedTask;
    }
}
