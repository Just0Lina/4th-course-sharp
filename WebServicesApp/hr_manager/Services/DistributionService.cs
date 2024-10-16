using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nsu.HackathonProblem.SharedData.Models;

namespace Nsu.HackathonProblem.HrManager.Services
{
    public class DistributionService : IDistributionService
    {
        private ConcurrentBag<Wishlist> _juniorPreferences = new();
        private ConcurrentBag<Wishlist> _teamLeadPreferences = new();
        private ConcurrentBag<Employee> _juniors = new();
        private ConcurrentBag<Employee> _teamLeads = new();

        public event Action OnAllPreferencesReceived;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITeamBuildingStrategy _teamBuildingStrategy;
        private readonly ILogger<DistributionService> _logger;
        private bool _finalDistributionSent = false;

        private List<Team> _teams;

        public DistributionService(ITeamBuildingStrategy teamBuildingStrategy,
            IHttpClientFactory httpClientFactory,
            ILogger<DistributionService> logger)
        {
            _teamBuildingStrategy = teamBuildingStrategy;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            OnAllPreferencesReceived += async () =>
            {
                if (_finalDistributionSent || !AllRequestsReceived()) return;
                _finalDistributionSent = true;
                await SendFinalDistribution();
            };
        }

        private bool AllRequestsReceived()
        {
            return _juniorPreferences.Count >= 5 && _teamLeadPreferences.Count >= 5;
        }

        public void SaveJuniorPreferences(RequestToHr request)
        {
            _juniorPreferences.Add(request.Wishlist);
            _juniors.Add(request.Employee);
            CheckIfAllPreferencesReceived();
        }

        public void SaveTeamLeadPreferences(RequestToHr request)
        {
            _teamLeadPreferences.Add(request.Wishlist);
            _teamLeads.Add(request.Employee);
            CheckIfAllPreferencesReceived();
        }

        private List<Team> BuildTeams()
        {
            _teams = _teamBuildingStrategy.BuildTeams(_teamLeads.ToList(),
                _juniors.ToList(),
                _juniorPreferences.ToList(),
                _teamLeadPreferences.ToList()).ToList();
            return _teams;
        }

        private void CheckIfAllPreferencesReceived()
        {
            _logger.LogInformation(
                $"Junior Preferences: {_juniorPreferences.Count}, Team Lead Preferences: {_teamLeadPreferences.Count}");

            if (_juniorPreferences.Count < 5 || _teamLeadPreferences.Count < 5) return;

            _logger.LogInformation("All preferences received, triggering OnAllPreferencesReceived event.");
            OnAllPreferencesReceived?.Invoke();
        }

        private TeamsAndPreferencesEntity GetAllPreferencesAsync()
        {
            var teams = BuildTeams();
            _logger.LogInformation("Current Teams:");
            foreach (var team in teams)
            {
                _logger.LogInformation($"Team Lead: {team.TeamLead.Name}, Junior: {team.Junior.Name}");
            }

            return new TeamsAndPreferencesEntity(_juniorPreferences.ToList(),
                _teamLeadPreferences.ToList(),
                teams);
        }

        private async Task<IActionResult> SendFinalDistribution()
        {
            var retryCount = 5;
            var delay = 2000;

            for (var i = 0; i < retryCount; i++)
            {
                try
                {
                    _logger.LogInformation("Sending final distribution...");

                    var allPreferences = GetAllPreferencesAsync();
                    var client = _httpClientFactory.CreateClient();

                    try
                    {
                        var response = await client.PostAsJsonAsync(
                            "http://hr_director:8080/api/hrdirector/calculate-harmony",
                            allPreferences);

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Distribution sent successfully.");
                            return new OkObjectResult("Distribution sent successfully to HR Director.");
                        }

                        _logger.LogWarning($"Failed to send distribution. Status code: {response.StatusCode}");
                        return new StatusCodeResult((int)response.StatusCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Request failed: {ex.Message}. Retrying in {delay / 1000} seconds...");
                        await Task.Delay(delay);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, $"Error during sending final distribution: {ex.Message}");
                    return new StatusCodeResult(500);
                }
            }

            _logger.LogError("Failed to send final distribution after multiple attempts.");
            return new StatusCodeResult(500);
        }
    }
}
