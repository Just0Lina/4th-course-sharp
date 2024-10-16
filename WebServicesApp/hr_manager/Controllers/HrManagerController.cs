using Microsoft.AspNetCore.Mvc;
using Nsu.HackathonProblem.HrManager.Services;
using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrManager.Controllers;

[ApiController]
[Route("api/hr")]
public class HrManagerController(IDistributionService distributionService)
    : ControllerBase
{
    [HttpPost("submit-junior-preferences")]
    public IActionResult SubmitJuniorPreferences(
        [FromBody] RequestToHr preferences)
    {
        distributionService.SaveJuniorPreferences(preferences);
        return Ok("Junior preferences received.");
    }

    [HttpPost("submit-teamlead-preferences")]
    public IActionResult SubmitTeamLeadPreferences(
        [FromBody] RequestToHr preferences)
    {
        distributionService.SaveTeamLeadPreferences(preferences);
        return Ok("Team lead preferences received.");
    }
}