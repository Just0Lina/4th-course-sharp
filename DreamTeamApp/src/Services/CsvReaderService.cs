using Nsu.HackathonProblem.Models;

namespace DreamTeamApp.Nsu.HackathonProblem.Services;

public class CsvReaderService
{
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