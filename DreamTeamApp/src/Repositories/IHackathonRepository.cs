using Nsu.HackathonProblem.Models;

namespace Nsu.HackathonProblem.Repositories;

public interface IHackathonRepository
{
    Task AddHackathonAsync(HackathonEntity hackathon);

    Task SavePreferencesAsync(List<EmployeePreferences> preferences, Role role,
        int hackathonId);

    Task<HackathonEntity> GetHackathonByIdAsync(int hackathonId);
    Task<IEnumerable<HackathonEntity>> GetAllHackathonsAsync();
    Task<double> CalculateAverageHarmonyAsync();
}