namespace Nsu.HackathonProblem.Contracts.Services
{
    using System.Collections.Generic;
    using System.IO;
    using Nsu.HackathonProblem.Contracts.Models;

    public class CsvReaderService
    {
        public List<Employee> ReadEmployees(string filePath)
        {
            var employees = new List<Employee>();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var values = line.Split(';');
                if (values.Length >= 2 && int.TryParse(values[0], out int id))
                {
                    var name = values[1];
                    employees.Add(new Employee(id, name));
                }
            }
            return employees;
        }
    }
}
