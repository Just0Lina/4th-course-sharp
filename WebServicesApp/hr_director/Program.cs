using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.HrDirector;
using Nsu.HackathonProblem.HrDirector.Database;
using Nsu.HackathonProblem.HrDirector.Repository;
using Nsu.HackathonProblem.HrDirector.Services;
using Nsu.HackathonProblem.SharedData.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<HackathonDbContext>(options =>
    options.UseNpgsql(connectionString).LogTo(Console.WriteLine, LogLevel.Warning));
builder.Services
    .AddScoped<IHarmonyCalculationService, HarmonyCalculationService>();
builder.Services.AddScoped<IHackathonRepository, HackathonRepository>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

builder.Services.AddSingleton<IConnection>(provider =>
{
    var factory = new ConnectionFactory() { HostName = "rabbitmq" };
    return factory.CreateConnection();
});

builder.Services.AddSingleton<IModel>(provider =>
{
    var connection = provider.GetRequiredService<IConnection>();
    return connection.CreateModel();
});

builder.Services.AddScoped<RabbitMqService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.ApplyMigrations(); 
}



app.UseRouting();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();