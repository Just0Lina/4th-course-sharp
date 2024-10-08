using Moq;
using Nsu.HackathonProblem.Core;
using Nsu.HackathonProblem.Models;
using static Nsu.HackathonProblem.Tests.Core.CommonMethods;

namespace Nsu.HackathonProblem.Tests.Core;

public class HrManagerTests
{
    [Fact]
    public void FormTeams_Should_Return_Correct_Number_Of_Teams()
    {
        var juniors = GetDefaultJuniors();
        var teamLeads = GetDefaultTeamLeads();

        var expectedTeams = GetDefaultExpectedTeams(teamLeads, juniors);
        var juniorsPreferencesList =
            GetEmployeePreferencesList(juniors, teamLeads);

        var teamLeadsPreferencesList =
            GetTeamLeadsPreferencesList(teamLeads, juniors);
        var hrManager = MockHrManager(juniors, teamLeads,
            juniorsPreferencesList, teamLeadsPreferencesList, expectedTeams);
        var (juniorPreferences, teamLeadPreferences) =
            hrManager.GetPreferences(juniors, teamLeads);
        var teams =
            hrManager.FormTeams(juniorPreferences, teamLeadPreferences);

        Assert.Equal(expectedTeams.Count, teams.Count);
    }

    [Fact]
    public void FormTeams_Should_Return_Predefined_Team_Distribution()
    {
        var juniors = GetDefaultJuniors();
        var teamLeads = GetDefaultTeamLeads();

        var expectedTeams = GetDefaultExpectedTeams(teamLeads, juniors);
        var juniorsPreferencesList =
            GetEmployeePreferencesList(juniors, teamLeads);

        var teamLeadsPreferencesList =
            GetTeamLeadsPreferencesList(teamLeads, juniors);
        var hrManager = MockHrManager(juniors, teamLeads,
            juniorsPreferencesList, teamLeadsPreferencesList, expectedTeams);

        var teams = hrManager.FormTeams(juniorsPreferencesList,
            teamLeadsPreferencesList);
        foreach (var team in teams)
        {
            Assert.Contains(team, expectedTeams);
        }
    }

    [Fact]
    public void FormTeams_Should_Call_TeamFormationStrategy_Once()
    {
        var juniors = GetDefaultJuniors();
        var teamLeads = GetDefaultTeamLeads();

        var expectedTeams = GetDefaultExpectedTeams(teamLeads, juniors);
        var juniorsPreferencesList =
            GetEmployeePreferencesList(juniors, teamLeads);

        var teamLeadsPreferencesList =
            GetTeamLeadsPreferencesList(teamLeads, juniors);
        var mockPreferencesService = MockPreferencesService(juniors, teamLeads,
            juniorsPreferencesList, teamLeadsPreferencesList);
        var mockTeamFormationService = MockTeamFormationService(expectedTeams);
        var mockHackathon =
            new Mock<Hackathon>(mockPreferencesService.Object);

        var hrManager = new HrManager(mockTeamFormationService.Object,
            mockHackathon.Object);
        var (juniorPreferences, teamLeadPreferences) =
            hrManager.GetPreferences(juniors, teamLeads);
        hrManager.FormTeams(juniorPreferences, teamLeadPreferences);

        mockTeamFormationService.Verify(
            s => s.FormTeams(It.IsAny<List<EmployeePreferences>>(),
                It.IsAny<List<EmployeePreferences>>()), Times.Once);
    }
}