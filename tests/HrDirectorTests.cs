using DreamTeamApp.Nsu.HackathonProblem.Core;
using DreamTeamApp.Nsu.HackathonProblem.Models;
using DreamTeamApp.Nsu.HackathonProblem.Services;
using DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;
using Moq;

namespace Tests;

using static CommonMethods;

public class HrDirectorTests
{
    [Fact]
    public void HarmonicMean_Of_IdenticalNumbers_Should_Equal_ThatNumber()
    {
        var service = new RatingService();
        var identicalTeams = new List<Team>
        {
            new(new Employee(1, "TeamLead1"), new Employee(2, "Junior1"),
                4, 4),
            new(new Employee(3, "TeamLead2"), new Employee(4, "Junior2"),
                4, 4)
        };

        var harmony = service.CalculateHarmonicMean(identicalTeams);

        Assert.Equal(4.0, harmony, precision: 2);
    }

    [Fact]
    public void HarmonicMean_Of_TwoAndSix_Should_Return_Three()
    {
        var service = new RatingService();
        var predefinedTeams = new List<Team>
        {
            new(new Employee(1, "TeamLead1"), new Employee(2, "Junior1"),
                2, 6),
        };

        var harmony = service.CalculateHarmonicMean(predefinedTeams);

        Assert.Equal(3.0, harmony, precision: 2);
    }

    [Fact]
    public void PredefinedTeams_Should_Return_PredefinedHarmonyValue()
    {
        var service = new RatingService();

        var predefinedTeams = new List<Team>
        {
            new Team(new Employee(1, "TeamLead1"), new Employee(2, "Junior1"),
                3, 4),
            new Team(new Employee(3, "TeamLead2"), new Employee(4, "Junior2"),
                2, 5)
        };

        var harmony = service.CalculateHarmonicMean(predefinedTeams);
        // 4/(1/3+1/4+1/2+1/5) == 3.1168
        Assert.Equal(3.12, harmony, precision: 2);
    }

    [Fact]
    public void HrManager_Should_Be_Called_Once_Per_Hackathon()
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

        var service = new Mock<IRatingService>();
        service.Setup(s =>
                s.CalculateHarmonicMean(expectedTeams))
            .Returns(3);
        var hrDirector = new HrDirector(service.Object, hrManager);


        hrDirector.OverseeHackathons(3, juniors, teamLeads);

        service.Verify(m => m.CalculateHarmonicMean(expectedTeams),
            Times.Exactly(3));
    }

    [Fact]
    public void
        Hackathon_With_Predefined_Participants_And_Preferences_Should_Give_Expected_Harmony()
    {
        var teamLead = new Employee(1, "TeamLead");
        var junior = new Employee(2, "Junior");

        var teamLeadPreferences = new Dictionary<Employee, int>
        {
            { junior, 1 }
        };

        var juniorPreferences = new Dictionary<Employee, int>
        {
            { teamLead, 1 }
        };

        var juniorPrefMock =
            new Mock<EmployeePreferences>(junior, juniorPreferences);
        var teamLeadPrefMock =
            new Mock<EmployeePreferences>(teamLead, teamLeadPreferences);

        var juniorsPreferencesList = new List<EmployeePreferences>
            { juniorPrefMock.Object };
        var teamLeadsPreferencesList = new List<EmployeePreferences>
            { teamLeadPrefMock.Object };

        var preferenceServiceMock = new Mock<IPreferencesService>();
        preferenceServiceMock.Setup(h =>
                h.GeneratePreferences(It.IsAny<List<Employee>>(),
                    It.IsAny<List<Employee>>()))
            .Returns((juniorsPreferencesList, teamLeadsPreferencesList));
        var mockHackathon = new Hackathon(preferenceServiceMock.Object);


        var teamFormationService = new TeamFormationService();

        var hrManager =
            new HrManager(teamFormationService, mockHackathon);
        var ratingService = new RatingService();
        var hrDirector = new HrDirector(ratingService, hrManager);
        var teams = hrManager.FormTeams([junior],
            [teamLead]);
        var harmony = hrDirector.CalculateOverallHarmony(teams);

        Assert.Equal(1.0, harmony);
    }
}