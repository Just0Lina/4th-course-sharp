using Newtonsoft.Json;

namespace Nsu.HackathonProblem.SharedData.Services;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class RabbitMqService : IRabbitMqService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqService()
    {
        var factory = new ConnectionFactory()
            { HostName = "rabbitmq" }; // Adjust the hostname as necessary
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "hackathon.start",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void Publish<T>(string queueName, T message)
    {
        _channel.QueueDeclare(queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var jsonMessage = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        _channel.BasicPublish(exchange: "",
            routingKey: queueName,
            basicProperties: null,
            body: body);
    }

    public void Consume<T>(string queueName, Func<T, Task> messageHandler)
    {
        _channel.QueueDeclare(queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var jsonMessage = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<T>(jsonMessage);

            if (message != null)
            {
                await messageHandler(message);
            }
        };

        _channel.BasicConsume(queue: queueName,
            autoAck: true,
            consumer: consumer);
    }

    public void Consume<T>(string queueName, Func<T, Task> onMessageReceived,
        CancellationToken cancellationToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var jsonMessage = Encoding.UTF8.GetString(body);
            var message = JsonConvert.DeserializeObject<T>(jsonMessage);

            if (message != null)
            {
                await onMessageReceived(message);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: true,
            consumer: consumer);
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}