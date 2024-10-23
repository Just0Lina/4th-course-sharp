namespace Nsu.HackathonProblem.SharedData.Services;

public interface IRabbitMqService
{
    void Publish<T>(string queueName, T message);

    void Consume<T>(string queueName, string exchangeName,
        Func<T, Task> messageHandler, CancellationToken cancellationToken);

}