using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Services;

public interface IHarmonyCalculationService
{
    

    Task<double> CalculateHarmonyAsync(List<Team> teams,
        int hackathonId);

    Task SaveHackathon(List<Team> teams, double harmonyIndex, int hackathonId);
}