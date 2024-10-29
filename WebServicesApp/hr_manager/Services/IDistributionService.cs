using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrManager.Services;

public interface IDistributionService
{
    void SaveTeamLeadPreferences(RequestToHr preferences);
    void SaveJuniorPreferences(RequestToHr preferences);
    void SetHackathonId(int messageHackathonId);
}