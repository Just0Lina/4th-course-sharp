using MassTransit;
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
    ILogger<HrDirectorController> logger,
    IBus publishEndpoint,
    IHackathonRepository hackathonRepository)
    : ControllerBase
{
    [HttpPost("calculate-harmony")]
    public async Task<IActionResult> CalculateHarmony(
        TeamsAndPreferencesEntity teamsAndPreferencesEntity)
    {
        logger.LogInformation($"id: {teamsAndPreferencesEntity.HackathonId}");

        var teams = teamsAndPreferencesEntity.Teams;
        var harmonyIndex =
            await harmonyService.CalculateHarmonyAsync(teams, teamsAndPreferencesEntity.HackathonId);
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
        var message = new HackathonAnnouncementMessage
        {
            HackathonId = request.HackathonId,
            Message = "Hackathon has started!",
            QueryId = Guid.NewGuid()
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
                await hackathonRepository
                    .ClearPreferencesAndTeamsForHackathonIdAsync(
                        request.HackathonId);
            }

            logger.LogInformation($"Sending announcement message: {message}");
            await publishEndpoint.Publish<HackathonAnnouncementMessage>(message,
                ctx =>
                {
                    ctx.SetRoutingKey(
                        "hackathonExchange"); 
                });
            logger.LogInformation($"Average hackathons harmony: {hackathonRepository.CalculateAverageHarmonyAsync()}");
            return Ok("Hackathon announcement sent successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to send announcement: {ex.Message}");
            return StatusCode(500,
                $"Failed to send announcement: {ex.Message}");
        }

    }
}