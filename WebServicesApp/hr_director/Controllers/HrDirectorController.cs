using Microsoft.AspNetCore.Mvc;
using Nsu.HackathonProblem.HrDirector.Repository;
using Nsu.HackathonProblem.HrDirector.Services;
using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;

namespace Nsu.HackathonProblem.HrDirector.Controllers;

[ApiController]
[Route("api/hrdirector")]
public class HrDirectorController(
    IHarmonyCalculationService harmonyService,
    RabbitMqService rabbitMqService,
    ILogger<HrDirectorController> logger,
    IHackathonRepository hackathonRepository)
    : ControllerBase
{
    [HttpPost("calculate-harmony")]
    public async Task<IActionResult> CalculateHarmony(
        TeamsAndPreferencesEntity teamsAndPreferencesEntity,
        CancellationToken cancellationToken)
    {
        logger.LogInformation($"id: {teamsAndPreferencesEntity.HackathonId}");

        Console.WriteLine("Calculating harmony for the teams and references");
        var teams = teamsAndPreferencesEntity.Teams;
        var harmonyIndex = await harmonyService.CalculateHarmonyAsync( teams, cancellationToken);
        logger.LogInformation($"Harmony calculated: {harmonyIndex}");
        logger.LogInformation($"id: {teamsAndPreferencesEntity.HackathonId}");

        await harmonyService.SaveHackathon(
            teams, harmonyIndex, teamsAndPreferencesEntity.HackathonId);

        return Ok($"Harmony calculated: {harmonyIndex}");
    }

    public class HackathonAnnouncementRequest
    {
        public int HackathonId { get; set; }
    }

    [HttpPost("announce-hackathon")]
    public async Task<IActionResult> AnnounceHackathon(
        [FromBody] HackathonAnnouncementRequest request)
    {
        rabbitMqService.DeleteQueues();
        var message = new HackathonAnnouncementMessage
        {
            HackathonId = request.HackathonId,
            Message = "Hackathon has started!"
        };

        try
        {
            if (await hackathonRepository.GetHackathonByIdAsync(
                    request.HackathonId) is null)
            {
                await hackathonRepository.SaveHackathonIdAsync(
                    request.HackathonId);
            }
            else
            {
                await hackathonRepository.ClearPreferencesAndTeamsForHackathonIdAsync(
                    request.HackathonId);
            }

            logger.LogInformation($"Sending announcement message {message}");
            rabbitMqService.Publish("hackathon.start", message);

            return
                Ok("Hackathon announcement sent successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                $"Failed to send announcement: {ex.Message}");
        }
    }
}