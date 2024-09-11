using DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;
using Moq;
using Nsu.HackathonProblem.Core;
using Nsu.HackathonProblem.Models;

namespace Nsu.HackathonProblem.Tests.Core;

public static class CommonMethods
{
    public static List<Employee> GetDefaultTeamLeads()
    {
        var teamLeads = new List<Employee>
            { new(3, "TeamLead1"), new(4, "TeamLead2") };
        return teamLeads;
    }

    public static List<Employee> GetDefaultJuniors()
    {
        var juniors = new List<Employee>
            { new(1, "Junior1"), new(2, "Junior2") };
        return juniors;
    }
    
    
    public static List<Team> GetDefaultExpectedTeams(List<Employee> teamLeads,
        List<Employee> juniors)
    {
        var expectedTeams = new List<Team>
        {
            new(teamLeads[0], juniors[0], 1, 1),
            new(teamLeads[1], juniors[1], 1, 1)
        };
        return expectedTeams;
    }

    public static List<EmployeePreferences> GetTeamLeadsPreferencesList(
        List<Employee> teamLeads, List<Employee> juniors)
    {
        var teamLeadsPreferencesList = new List<EmployeePreferences>
        {
            new(teamLeads[0],
                new Dictionary<Employee, int>
                    { { juniors[0], 1 }, { juniors[1], 2 } }),
            new(teamLeads[1],
                new Dictionary<Employee, int>
                    { { juniors[1], 1 }, { juniors[0], 2 } })
        };
        return teamLeadsPreferencesList;
    }

    public static List<EmployeePreferences> GetEmployeePreferencesList(
        List<Employee> juniors, List<Employee> teamLeads)
    {
        var juniorsPreferencesList = new List<EmployeePreferences>
        {
            new(juniors[0],
                new Dictionary<Employee, int>
                    { { teamLeads[0], 1 }, { teamLeads[1], 2 } }),
            new(juniors[1],
                new Dictionary<Employee, int>
                    { { teamLeads[1], 1 }, { teamLeads[0], 2 } })
        };
        return juniorsPreferencesList;
    }

    public static HrManager MockHrManager(List<Employee> juniors,
        List<Employee> teamLeads,
        List<EmployeePreferences> juniorsPreferencesList,
        List<EmployeePreferences> teamLeadsPreferencesList,
        List<Team> expectedTeams)
    {
        var mockPreferencesService = MockPreferencesService(juniors, teamLeads,
            juniorsPreferencesList, teamLeadsPreferencesList);
        var mockTeamFormationService = MockTeamFormationService(expectedTeams);
        var mockHackathon =
            new Mock<Hackathon>(mockPreferencesService.Object);

        var hrManager = new HrManager(mockTeamFormationService.Object,
            mockHackathon.Object);
        return hrManager;
    }

    public static Mock<ITeamFormationService> MockTeamFormationService(
        List<Team> expectedTeams)
    {
        var mockTeamFormationService = new Mock<ITeamFormationService>();

        mockTeamFormationService.Setup(s =>
                s.FormTeams(It.IsAny<List<EmployeePreferences>>(),
                    It.IsAny<List<EmployeePreferences>>()))
            .Returns(expectedTeams);
        return mockTeamFormationService;
    }

    public static Mock<IPreferencesService> MockPreferencesService(
        List<Employee> juniors, List<Employee> teamLeads,
        List<EmployeePreferences> juniorsPreferencesList,
        List<EmployeePreferences> teamLeadsPreferencesList)
    {
        var mockPreferencesService = new Mock<IPreferencesService>();

        mockPreferencesService
            .Setup(h => h.GeneratePreferences(juniors, teamLeads))
            .Returns((juniorsPreferencesList, teamLeadsPreferencesList));
        return mockPreferencesService;
    }
}