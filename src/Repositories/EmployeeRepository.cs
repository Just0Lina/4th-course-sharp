using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.Database;
using Nsu.HackathonProblem.Models;

namespace Nsu.HackathonProblem.Repositories;

public class EmployeeRepository(HackathonDbContext context)
    : IEmployeeRepository
{
    public async Task<Employee> GetEmployeeByIdAsync(int employeeId, Role role)
    {
        return await context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.Role == role);
    }
}