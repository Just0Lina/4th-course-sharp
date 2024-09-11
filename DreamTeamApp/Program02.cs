using Nsu.HackathonProblem.Contracts.Services;
using Nsu.HackathonProblem.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

class Program02
{
    static void Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
        
            .ConfigureServices((_, services) =>
            {
                services.AddHostedService<HackathonWorker>();

                services.AddTransient<IPreferencesService, PreferencesService>();
                services.AddTransient<ITeamFormationService, TeamFormationService>();
                services.AddTransient<IRatingService, RatingService>();

                services.AddTransient<Hackathon>();
                services.AddTransient<HrManager>();
                services.AddTransient<HrDirector>();
            })
            .Build();

        host.Run();
    }
}
