using System.Collections.Concurrent;
using System.Text.Json;
using MassTransit;
using Nsu.HackathonProblem.HrDirector.Database;
using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;

namespace Nsu.HackathonProblem.HrDirector.Services;

public class PreferencesConsumer(
    ILogger<PreferencesConsumer> logger,
    IServiceScopeFactory _serviceScopeFactory)
    : IConsumer<PreferencesMessage>
{
    public static event Action<PreferencesMessage> PreferencesReceivedTcs;

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

        await SavePreferencesToDatabaseAsync(message);
    }


    private async Task SavePreferencesToDatabaseAsync(
        PreferencesMessage message)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext =
            scope.ServiceProvider.GetRequiredService<HackathonDbContext>();

        for (int priority = 0;
             priority < message.Preferences.DesiredEmployees.Length;
             priority++)
        {
            var preferenceEntity = new EmployeePreferenceEntity
            {
                HackathonId = message.HackathonId,
                Role = message.EmployeeType == "junior"
                    ? Role.Junior
                    : Role.TeamLead,
                EmployeeId = message.Employee.Id,
                PreferredEmployeeId =
                    message.Preferences.DesiredEmployees[priority],
                Priority = priority
            };

            dbContext.EmployeePreferences.Add(preferenceEntity);
            await dbContext.SaveChangesAsync();
            logger.LogInformation(
                $"Saved preferences for HackathonId: {message.HackathonId}, EmployeeId: {message.Employee.Id}, EmployeeType: {message.EmployeeType}, PreferredEmployeeId: {preferenceEntity.PreferredEmployeeId}, Priority: {preferenceEntity.Priority}");
        }
    }
}