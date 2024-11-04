using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Nsu.HackathonProblem.SharedData.Models;
using RabbitMQ.Client.Exceptions;

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
        private int HackathonId;

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
                await SendFinalDistribution(HackathonId);
            };
        }

        public bool AllRequestsReceived()
        {
            return _juniorPreferences.Count >= 5 &&
                   _teamLeadPreferences.Count >= 5;
        }

        public void SaveJuniorPreferences(RequestToHr request)
        {
            _juniorPreferences.Add(request.Wishlist);
            _juniors.Add(request.Employee);
            CheckIfAllPreferencesReceived();
        }

        public void SetHackathonId(int messageHackathonId)
        {
            HackathonId = messageHackathonId;
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
            _logger.LogDebug(
                $"Junior Preferences: {_juniorPreferences.Count}, Team Lead Preferences: {_teamLeadPreferences.Count}");

            if (!AllRequestsReceived()) return;

            _logger.LogDebug(
                "All preferences received, triggering OnAllPreferencesReceived event.");
            OnAllPreferencesReceived?.Invoke();
        }

        private TeamsAndPreferencesEntity GetAllPreferencesAsync(int hackathonId)
        {
            var teams = BuildTeams();
            _logger.LogDebug("Current Teams:");
            foreach (var team in teams)
            {
                _logger.LogDebug(
                    $"Team Lead: {team.TeamLead.Name}, Junior: {team.Junior.Name}");
            }

            return new TeamsAndPreferencesEntity(teams, hackathonId);
        }

        private void ClearAllPreferences()
        {
            _finalDistributionSent = false;
            _juniors.Clear();
            _teams.Clear();
            _teamLeads.Clear();
            _teamLeadPreferences.Clear();
            _juniorPreferences.Clear();
        }

        private async Task<IActionResult> SendFinalDistribution(int hackathonId)
        {
            _logger.LogDebug($"Sending final distribution... {hackathonId}");

            var allPreferences =
                GetAllPreferencesAsync(hackathonId);
            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.PostAsJsonAsync(
                    "http://hr_director:8080/api/hrdirector/calculate-harmony",
                    allPreferences);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Distribution sent successfully.");
                    ClearAllPreferences();
                    return new OkObjectResult(
                        "Distribution sent successfully to HR Director.");
                }

                ClearAllPreferences();
                _logger.LogWarning(
                    $"Failed to send distribution. Status code: {response.StatusCode}");
                return new StatusCodeResult((int)response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    $"Error during sending final distribution: {ex.Message}");
                ClearAllPreferences();
                return new StatusCodeResult(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Request failed: {ex.Message}");
                ClearAllPreferences();
                return new StatusCodeResult(500);
            }
        }
    }
}