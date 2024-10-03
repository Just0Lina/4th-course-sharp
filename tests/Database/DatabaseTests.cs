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
        var hackathon = new HackathonEntity { Harmony = 90.0m };
        await _repository.AddHackathonAsync(hackathon);
        await _context.SaveChangesAsync();

        var retrievedHackathon =
            await _repository.GetHackathonByIdAsync(hackathon.Id);

        Assert.NotNull(retrievedHackathon);
        Assert.Equal(hackathon.Harmony, retrievedHackathon.Harmony);
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