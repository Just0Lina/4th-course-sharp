using Nsu.HackathonProblem.SharedData.Services;
using Nsu.HackathonProblem.TeamLead.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IPreferencesService, PreferencesService>();
builder.Services.AddHostedService<StartupService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();