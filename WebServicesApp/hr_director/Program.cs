using MassTransit;
using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.HrDirector;
using Nsu.HackathonProblem.HrDirector.Database;
using Nsu.HackathonProblem.HrDirector.Repository;
using Nsu.HackathonProblem.HrDirector.Services;
using Nsu.HackathonProblem.SharedData.Models;
using Nsu.HackathonProblem.SharedData.Services;
using Nsu.HackathonProblem.TeamLead.Service;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddDbContextFactory<HackathonDbContext>(options =>
    options.UseNpgsql(connectionString), ServiceLifetime.Scoped);

builder.Services.AddScoped<IHarmonyCalculationService, HarmonyCalculationService>();
builder.Services.AddScoped<IHackathonRepository, HackathonRepository>();
builder.Services.AddSingleton<IHackathonStartedHandler, HackathonStartedHandler>();


builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PreferencesConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        
        cfg.ReceiveEndpoint("hr_director", e =>
        {
            e.Bind("preferencesExchange");
            e.ConfigureConsumer<PreferencesConsumer>(context);
        });
        cfg.UseRetry(retry =>
        {
            retry.Interval(3, TimeSpan.FromSeconds(10)); 
        });
        
    });
});




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.ApplyMigrations();
    
}

app.UseRouting();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();

