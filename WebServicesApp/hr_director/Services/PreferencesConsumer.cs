using System.Collections.Concurrent;
using MassTransit;
using Nsu.HackathonProblem.HrDirector.Repository;
using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Services;

public class PreferencesConsumer(
    ILogger<PreferencesConsumer> logger,
    IHackathonRepository hackathonRepository)
    : IConsumer<PreferencesMessage>
{
    private static readonly ConcurrentDictionary<string, bool> ProcessedPreferences =
        new ConcurrentDictionary<string, bool>();

    public async Task Consume(ConsumeContext<PreferencesMessage> context)
    {
        var message = context.Message;

        var uniqueKey =
            $"{message.HackathonId}-{message.Employee.Id}--{message.EmployeeType}";

        if (!ProcessedPreferences.TryAdd(uniqueKey, true))
        {
            logger.LogWarning(
                $"Duplicate preference message received for HackathonId: {message.HackathonId}, EmployeeId: {message.Employee.Id}, EmployeeType: {message.EmployeeType}");
            return;
        }

        logger.LogInformation(
            $"Processing preferences for HackathonId: {message.HackathonId}, EmployeeId: {message.Employee.Id}, EmployeeType: {message.EmployeeType}");

        await hackathonRepository.SavePreferencesToDatabaseAsync(message);
    }

    
}