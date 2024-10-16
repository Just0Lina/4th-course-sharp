using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrDirector.Repository;

public interface IHackathonRepository
{
    Task AddHackathonAsync(HackathonEntity hackathon);

    Task SavePreferencesAsync(List<Wishlist> preferences, Role role,
        int hackathonId);

    Task SaveEmployeesAsync(List<Team> teams);
    
}