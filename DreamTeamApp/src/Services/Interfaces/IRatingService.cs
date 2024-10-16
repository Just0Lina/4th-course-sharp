using Nsu.HackathonProblem.Models;

namespace DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;

public interface IRatingService
{
    double CalculateHarmonicMean(List<Team> teams);
}