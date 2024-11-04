using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;

namespace Nsu.HackathonProblem.HrManager.Services;

public class PreferencesMessageConsumer(
    IDistributionService distributionService,
    ILogger<PreferencesMessageConsumer> logger)
    : IConsumer<PreferencesMessage>
{
    public async Task Consume(ConsumeContext<PreferencesMessage> context)
    {
        var message = context.Message;
        if (distributionService.AllRequestsReceived()) return;

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

        logger.LogDebug(
            $"Received message: {JsonSerializer.Serialize(message)}");
    }
}