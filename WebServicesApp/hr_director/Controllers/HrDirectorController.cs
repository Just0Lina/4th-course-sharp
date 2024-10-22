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
    RabbitMqService _rabbitMqService)
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
        Console.WriteLine($"Harmony calculated: {harmonyIndex}");
        await harmonyService.SaveHackathon(juniorPreferences,
            teamLeadPreferences,
            teams, harmonyIndex);

        return Ok($"Harmony calculated: {harmonyIndex}");
    }

    [HttpPost("announce-hackathon")]
    public Task<IActionResult> AnnounceHackathon(
        [FromBody] string hackathonId)
    {
        var message = new HackathonAnnouncementMessage
        {
            HackathonId = hackathonId,
            Message = "Hackathon has started!"
        };

        try
        {
            _rabbitMqService.Publish("hackathon.start", message);
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