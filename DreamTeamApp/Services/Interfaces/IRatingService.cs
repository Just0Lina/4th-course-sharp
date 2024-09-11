using Nsu.HackathonProblem.Contracts.Models;

namespace Nsu.HackathonProblem.Contracts.Services;

public interface IRatingService
{
    double CalculateHarmonicMean(List<Team> teams);
}