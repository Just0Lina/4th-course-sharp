using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client.Exceptions;

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
    string username = "guest"; 
    string password = "guest";

    public RabbitMqService()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        DeclareExchanges();
    }

    private void DeclareExchanges()
    {
        _channel.ExchangeDeclare(exchange: "hackathon.start",
            type: ExchangeType.Fanout);
        _channel.ExchangeDeclare(exchange: "preferences.submit",
            type: ExchangeType.Fanout); 
    }

    public void Publish<T>(string exchangeName, T message)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        _channel.BasicPublish(exchange: exchangeName, routingKey: "",
            basicProperties: null, body: body);
    }

    public async void Consume<T>(string queueName, string exchangeName,
        Func<T, Task> messageHandler, CancellationToken cancellationToken)
    {
        _channel.QueueDeclare(queue: queueName, durable: false,
            exclusive: false, autoDelete: false, arguments: null);

        _channel.QueueBind(queue: queueName, exchange: exchangeName,
            routingKey: "");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var jsonMessage = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<T>(jsonMessage);

            if (message != null)
            {
                await messageHandler(message);
                _channel.BasicAck(ea.DeliveryTag, false);

            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false,
            consumer: consumer);
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }


    private async Task<List<string>> GetAllQueues()
    {
        using var httpClient = new HttpClient();
        var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var response =
            await httpClient.GetStringAsync(
                "http://rabbitmq:15672/api/queues");
        Console.WriteLine(response);
        var queues = JArray.Parse(response);
        var queueNames = new List<string>();

        foreach (var queue in queues)
        {
            queueNames.Add(queue["name"].ToString());
        }

        return queueNames;
    }

    public async Task DeleteQueues()
    {
        var queues = await GetAllQueues();

        foreach (var queue in queues)
        {
            Console.WriteLine($"'{queue}'");
            try
            {
                _channel.QueuePurge(queue);
                Console.WriteLine($"Queue '{queue}' deleted successfully.");
            }
            catch (RabbitMQClientException ex)
            {
                Console.WriteLine($"Queue '{queue}' not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error deleting queue '{queue}': {ex.Message}");
            }
        }
    }
}