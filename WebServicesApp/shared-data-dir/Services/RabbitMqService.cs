using Newtonsoft.Json;

namespace Nsu.HackathonProblem.SharedData.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqService()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq" // Настройте имя хоста при необходимости
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Объявление обменов
        DeclareExchanges();
    }

    private void DeclareExchanges()
    {
        _channel.ExchangeDeclare(exchange: "hackathon.start", type: ExchangeType.Fanout);
        _channel.ExchangeDeclare(exchange: "preferences.submit", type: ExchangeType.Fanout); // Обмен для очереди preferences.submit
    }

    public void Publish<T>(string exchangeName, T message)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        // Публикация в указанный обмен
        _channel.BasicPublish(exchange: exchangeName, routingKey: "", basicProperties: null, body: body);
    }

    public void Consume<T>(string queueName, string exchangeName, Func<T, Task> messageHandler, CancellationToken cancellationToken)
    {
        // Объявление очереди
        _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        
        // Привязка очереди к обмену
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");

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

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}
