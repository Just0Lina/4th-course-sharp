// using System;
// using System.Collections.Generic;
// using Nsu.HackathonProblem.Contracts;
// using Nsu.HackathonProblem.Contracts.Services;
// using Nsu.HackathonProblem.Contracts.Models;
// using Nsu.HackathonProblem.Core;

// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;


// class Program01
// {
//     static void Main(string[] args)
//     {
//         var csvReaderService = new CsvReaderService();
//         var preferencesService = new PreferencesService();
//         var teamFormationService = new TeamFormationService();

//         var juniors = csvReaderService.ReadEmployees("Resources/Juniors20.csv");
//         var teamLeads = csvReaderService.ReadEmployees("Resources/Teamleads20.csv");

//         double totalHarmonicMean = 0;
//         int hackathonCount = 10;

//         for (int i = 1; i <= hackathonCount; i++)
//         {
//             Console.WriteLine($"--- Hackathon #{i} ---");

//             var juniorPreferences = preferencesService.CreatePreferences(juniors, teamLeads);
//             var teamLeadPreferences = preferencesService.CreatePreferences(teamLeads, juniors);

//             var teams = teamFormationService.FormTeams(juniorPreferences, teamLeadPreferences);

//             //DisplayGroupPreferences(teams);

//             double harmonicMean = teamFormationService.CalculateOverallHarmony(teams);
//             Console.WriteLine($"Harmonic Mean Satisfaction for Hackathon #{i}: {harmonicMean:F2}");

//             totalHarmonicMean += harmonicMean;
//         }

//         double averageHarmonicMean = totalHarmonicMean / hackathonCount;
//         Console.WriteLine($"Average Harmonic Mean Satisfaction after {hackathonCount} hackathons: {averageHarmonicMean:F2}");
//     }


//     static void DisplayPreferences(List<EmployeePreferences> juniorPreferences, List<EmployeePreferences> teamLeadPreferences)
//     {
//         Console.WriteLine("Junior Preferences:");
//         foreach (var jp in juniorPreferences)
//         {
//             Console.WriteLine($"{jp.Junior.Name}'s preferences:");
//             foreach (var tl in jp.PreferredEmployees)
//             {
//                 Console.WriteLine($"- {tl.Key.Name} (Priority: {tl.Value})");
//             }
//             Console.WriteLine();
//         }

//         Console.WriteLine("Team Lead Preferences:");
//         foreach (var tlp in teamLeadPreferences)
//         {
//             Console.WriteLine($"{tlp.TeamLead.Name}'s preferences:");
//             foreach (var j in tlp.PreferredEmployees)
//             {
//                 Console.WriteLine($"- {j.Key.Name} (Priority: {j.Value})");
//             }
//             Console.WriteLine();
//         }
//     }

//     static void DisplayGroupPreferences(List<Team> teams)
//     {
//         Console.WriteLine("Вот такие вот вышли пары:");
//         foreach (var team in teams)
//         {
//             Console.WriteLine($"Джун {team.Junior.Name} с лидом {team.TeamLead.Name}");
//             Console.WriteLine($"Предпочтения Джуна: {team.JuniorPriority}, Предпочтения Лида: {team.TeamLeadPriority}");
//         }
//     }
// }
