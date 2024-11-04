using MassTransit;
using Nsu.HackathonProblem.HrManager.Services;
using Nsu.HackathonProblem.SharedData.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITeamBuildingStrategy, TeamBuildingStrategy>();
builder.Services.AddSingleton<IDistributionService, DistributionService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PreferencesMessageConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ReceiveEndpoint("hr_manager", e =>
        {
            e.Bind("preferencesExchange");
            e.ConfigureConsumer<PreferencesMessageConsumer>(context);
        });
        
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();