using Microsoft.AspNetCore.Mvc;
using Nsu.HackathonProblem.HrDirector.Services;
using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Controllers;

[ApiController]
[Route("api/hrdirector")]
public class HrDirectorController(IHarmonyCalculationService harmonyService)
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
        await harmonyService.SaveHackathon(juniorPreferences, teamLeadPreferences,
            teams, harmonyIndex);

        return Ok($"Harmony calculated: {harmonyIndex}");
    }
}