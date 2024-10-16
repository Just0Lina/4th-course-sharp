using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.SharedData.Services;

public static class DataService
{
    public static readonly string JuniorsCsv = "shared-data/Resources/Juniors5.csv";
    public static readonly string teamLeadsCsv = "shared-data/Resources/Teamleads5.csv";
    public static Employee ReadEmployeeById(string filePath, int id)
    {
        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            var values = line.Split(';');
            if (values.Length < 2 ||
                !int.TryParse(values[0], out var employeeId))
                continue;

            if (employeeId == id)
            {
                var name = values[1];
                return new Employee(employeeId, name);
            }
        }

        return null;
    }

    public static List<Employee> ReadEmployees(string filePath)
    {
        var employees = new List<Employee>();
        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            var values = line.Split(';');
            if (values.Length < 2 || !int.TryParse(values[0], out var id))
                continue;
            var name = values[1];
            employees.Add(new Employee(id, name));
        }

        return employees;
    }
}