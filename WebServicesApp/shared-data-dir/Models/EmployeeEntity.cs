namespace Nsu.HackathonProblem.SharedData.Models;

public record EmployeeEntity(int Id, string Name, Role Role)
    : Employee(Id, Name)
{
    public Role Role { get; set; } = Role;
}