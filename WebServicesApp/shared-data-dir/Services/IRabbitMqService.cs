namespace Nsu.HackathonProblem.SharedData.Services;

public interface IRabbitMqService
{
    void Publish<T>(string queueName, T message);
    void Consume<T>(string queueName, Func<T, Task> messageHandler);
    void Consume<T>(string hackathonStart,Func<T, Task> messageHandler, CancellationToken stoppingToken);
}