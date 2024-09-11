using Nsu.HackathonProblem.Contracts.Models;
using Nsu.HackathonProblem.Contracts.Services;

namespace Tests;

public class PreferencesTests
{
    [Fact]
    public void PreferencesList_Size_Should_Match_Number_Of_TeamLeads_And_Juniors()
    {
        var teamLeads = new List<Employee>
        {
            new(1, "TeamLead1"),
            new(2, "TeamLead2")
        };
        var juniors = new List<Employee>
        {
            new(1, "Junior1"),
            new(2, "Junior2")
        };
        var service = new PreferencesService();

        var wishlist = service.CreatePreferences(juniors, teamLeads);

        Assert.Equal(teamLeads.Count, wishlist.Count);
    }

    [Fact]
    public void Each_Junior_Should_Have_All_TeamLeads_In_Their_Preferences()
    {
        var teamLeads = new List<Employee>
        {
            new(1, "TeamLead1"),
            new(2, "TeamLead2")
        };
        var juniors = new List<Employee>
        {
            new(1, "Junior1"),
            new(2, "Junior2")
        };
        var service = new PreferencesService();

        var wishlist = service.CreatePreferences(juniors, teamLeads);

        foreach (var juniorWishList in wishlist)
        {
            foreach (var teamLead in teamLeads)
            {
                Assert.Contains(teamLead, juniorWishList.PreferredEmployees);
            }
        }
    }
}