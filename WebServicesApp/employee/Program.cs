using MassTransit;
using Nsu.HackathonProblem.SharedData.Services;
using Nsu.HackathonProblem.TeamLead.Configurations;
using Nsu.HackathonProblem.TeamLead.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IPreferencesService, PreferencesService>();
builder.Services.AddHostedService<HackathonStartConsumer>();
builder.Configuration.AddEnvironmentVariables();
var employeeSettingsDto = builder.Configuration.GetSection("EmployeeSettings")
    .Get<EmployeeSettings>()!;
builder.Services.AddSingleton(employeeSettingsDto);
builder.Services
    .AddSingleton<IHackathonStartedHandler, HackathonStartedHandler>();
builder.Logging.AddDebug();
builder.Services.AddSingleton<HackathonStartConsumer>();

builder.Services.AddHostedService<StartupService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<HackathonAnnouncementConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        
        cfg.Host("rabbitmq", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ReceiveEndpoint($"Employee-{employeeSettingsDto.EmployeeType}-{employeeSettingsDto.EmployeeId}", e => 
        {
            e.Bind("hackathonExchange"); 

            e.ConfigureConsumers(context);
            
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
}

app.UseRouting();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();