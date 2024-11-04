using MassTransit;
using Microsoft.Extensions.Hosting;
using Nsu.HackathonProblem.SharedData.Models;
using Microsoft.Extensions.Logging;

namespace Nsu.HackathonProblem.TeamLead.Service;
public class HackathonStartConsumer(
    IBusControl bus,
    ILogger<HackathonAnnouncementConsumer> logger,
    IHackathonStartedHandler hackathonStartedHandler)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bus.ConnectReceiveEndpoint("hackathon.start", e =>
        {
            e.Consumer(() => new HackathonAnnouncementConsumer(logger, hackathonStartedHandler));
        });

        return Task.CompletedTask;
    }
    
}

public class HackathonAnnouncementConsumer(
    ILogger<HackathonAnnouncementConsumer> logger,
    IHackathonStartedHandler hackathonStartedHandler)
    : IConsumer<HackathonAnnouncementMessage>
{
    private static readonly HashSet<Guid> ProcessedQueryIds = new HashSet<Guid>();

    public async Task Consume(ConsumeContext<HackathonAnnouncementMessage> context)
    {
        var message = context.Message;

        if (!ProcessedQueryIds.Add(message.QueryId))
        {
            logger.LogWarning($"Duplicate message received for QueryId: {message.QueryId}");
            return; 
        }

        logger.LogInformation($"Received hackathon announcement: {message.Message} for Hackathon ID: {message.HackathonId}");

        hackathonStartedHandler.OnHackathonStarted(message);
    }
}


public interface IHackathonStartedHandler
{
    void OnHackathonStarted(HackathonAnnouncementMessage message);
}

public class HackathonStartedHandler : IHackathonStartedHandler
{
    public static event Action<HackathonAnnouncementMessage> HackathonStarted;

    public void OnHackathonStarted(HackathonAnnouncementMessage message)
    {
        HackathonStarted?.Invoke(message);
    }
}
