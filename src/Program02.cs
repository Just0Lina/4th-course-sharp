using DreamTeamApp.Nsu.HackathonProblem.Services;
using DreamTeamApp.Nsu.HackathonProblem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.Core;
using Nsu.HackathonProblem.Database;
using Nsu.HackathonProblem.Workers;

class Program02
{
    static void Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args).ConfigureLogging(
                (context, logging) =>
                {
                    var config = context.Configuration.GetSection("Logging");
                    logging.AddConfiguration(config);
                    logging.AddConsole();
                    logging.AddFilter(
                        "Microsoft.EntityFrameworkCore.Database.Command",
                        LogLevel.Warning);
                })
            .ConfigureServices((_, services) =>
            {
                services.AddHostedService<HackathonWorker>();
                services.AddTransient<EmployeeService>();

                services
                    .AddTransient<IPreferencesService, PreferencesService>();
                services
                    .AddTransient<ITeamFormationService,
                        TeamFormationService>();
                services.AddTransient<IRatingService, RatingService>();

                services.AddDbContext<HackathonDbContext>(options =>
                    options.UseNpgsql(
                            "Host=localhost;Port=5432;Database=hackathon_db;Username=postgres;Password=postgres;SearchPath=public")
                        .LogTo(Console.WriteLine,
                            new[] { DbLoggerCategory.Database.Command.Name },
                            LogLevel.Warning));
                services.AddTransient<Hackathon>();
                services.AddTransient<HrManager>();
                services.AddTransient<HrDirector>();
            })
            .Build();

        host.Run();
    }
}