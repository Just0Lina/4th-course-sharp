using Microsoft.AspNetCore.Mvc;
using Nsu.HackathonProblem.HrDirector.Services;
using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;
using Nsu.HackathonProblem.TeamLead.Service;

namespace Nsu.HackathonProblem.HrDirector.Controllers;

[ApiController]
[Route("api/hrdirector")]
public class HrDirectorController(
    IHarmonyCalculationService harmonyService,
    RabbitMqService rabbitMqService,
    ILogger<HrDirectorController> logger)
    : ControllerBase
{
    [HttpPost("calculate-harmony")]
    public async Task<IActionResult> CalculateHarmony(
        TeamsAndPreferencesEntity teamsAndPreferencesEntity)
    {
        var teams = teamsAndPreferencesEntity.Teams;
        var juniorPreferences = teamsAndPreferencesEntity.JuniorPreferences;
        var teamLeadPreferences = teamsAndPreferencesEntity.TeamLeadPreferences;
        var harmonyIndex = harmonyService.CalculateHarmony(juniorPreferences,
            teamLeadPreferences, teams);
        logger.LogInformation($"Harmony calculated: {harmonyIndex}");
        await harmonyService.SaveHackathon(juniorPreferences,
            teamLeadPreferences,
            teams, harmonyIndex);

        return Ok($"Harmony calculated: {harmonyIndex}");
    }

    public class HackathonAnnouncementRequest
    {
        public long HackathonId { get; set; }
    }

    [HttpPost("announce-hackathon")]
    public Task<IActionResult> AnnounceHackathon(
        [FromBody] HackathonAnnouncementRequest request)
    {
        var message = new HackathonAnnouncementMessage
        {
            HackathonId = request.HackathonId,
            Message = "Hackathon has started!"
        };

        try
        {
            logger.LogInformation("Sending announcement message");
            rabbitMqService.Publish("hackathon.start", message);
            return Task.FromResult<IActionResult>(
                Ok("Hackathon announcement sent successfully."));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IActionResult>(StatusCode(500,
                $"Failed to send announcement: {ex.Message}"));
        }
    }
}