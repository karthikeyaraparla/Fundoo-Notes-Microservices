using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class RabbitMqPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqPublisher()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        _connection = factory.CreateConnection();  
        _channel = _connection.CreateModel();      

        _channel.ExchangeDeclare(
            exchange: "collab_exchange",
            type: ExchangeType.Fanout
        );

        Console.WriteLine("RabbitMQ Connected");
    }

    public void Publish(object message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: "collab_exchange",
            routingKey: "",
            basicProperties: null,
            body: body
        );

        Console.WriteLine("Event Published");
    }
}