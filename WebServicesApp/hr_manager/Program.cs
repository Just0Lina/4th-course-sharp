using Nsu.HackathonProblem.HrManager.Services;
using Nsu.HackathonProblem.SharedData.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITeamBuildingStrategy, TeamBuildingStrategy>();
builder.Services.AddSingleton<IDistributionService, DistributionService>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddHostedService<PreferencesConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();