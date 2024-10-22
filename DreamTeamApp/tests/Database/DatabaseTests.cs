using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.Database;
using Nsu.HackathonProblem.Models;
using Nsu.HackathonProblem.Repositories;

namespace Nsu.HackathonProblem.Tests.Database;

public class HackathonRepositoryTests
{
    private readonly HackathonDbContext _context;
    private readonly IHackathonRepository _repository;

    public HackathonRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<HackathonDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new HackathonDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _repository = new HackathonRepository(_context);
    }

    [Fact]
    public async Task AddHackathon_Should_SaveToDatabase()
    {
        var hackathon = new HackathonEntity { Harmony = 85.5m };

        await _repository.AddHackathonAsync(hackathon);
        await _context.SaveChangesAsync();

        var savedHackathon =
            await _repository.GetHackathonByIdAsync(hackathon.Id);
        Assert.NotNull(savedHackathon);
        Assert.Equal(hackathon.Harmony, savedHackathon.Harmony);
    }

    [Fact]
    public async Task GetHackathon_Should_ReturnCorrectHackathon()
    {
        var team1 = new TeamEntity { TeamLeadId = 1, JuniorId = 2 };
        var team2 = new TeamEntity { TeamLeadId = 3, JuniorId = 4 };
        var hackathon = new HackathonEntity { Harmony = 90.0m };

        hackathon.Teams = new List<TeamEntity> { team1, team2 };

        await _repository.AddHackathonAsync(hackathon);
        await _context.SaveChangesAsync();

        var retrievedHackathon =
            await _repository.GetHackathonByIdAsync(hackathon.Id);

        Assert.NotNull(retrievedHackathon);
        Assert.Equal(hackathon.Harmony, retrievedHackathon.Harmony);
        Assert.Equal(hackathon.Id, retrievedHackathon.Id);
        Assert.Equal(team1.TeamLeadId, retrievedHackathon.Teams[0].TeamLeadId);
        Assert.Equal(team1.JuniorId, retrievedHackathon.Teams[0].JuniorId);
        Assert.Equal(team2.TeamLeadId, retrievedHackathon.Teams[1].TeamLeadId);
        Assert.Equal(team2.JuniorId, retrievedHackathon.Teams[1].JuniorId);
        Assert.Equal(hackathon.Teams.Count, retrievedHackathon.Teams.Count);
    }

    [Fact]
    public async Task CalculateAverageHarmony_Should_CalculateAndStoreAverage()
    {
        var hackathons = new List<HackathonEntity>
        {
            new HackathonEntity { Harmony = 80.0m },
            new HackathonEntity { Harmony = 85.0m },
            new HackathonEntity { Harmony = 90.0m }
        };

        foreach (var hackathon in hackathons)
        {
            await _repository.AddHackathonAsync(hackathon);
        }

        await _context.SaveChangesAsync();

        var averageHarmony = await _repository.CalculateAverageHarmonyAsync();

        Assert.Equal(85.0d, averageHarmony);
    }
}