using Nsu.HackathonProblem.Models;

namespace Nsu.HackathonProblem.Repositories;

public interface IEmployeeRepository
{
    Task<Employee> GetEmployeeByIdAsync(int employeeId, Role role);
}