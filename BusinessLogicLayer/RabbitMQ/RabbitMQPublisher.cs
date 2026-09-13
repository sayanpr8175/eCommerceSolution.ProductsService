using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;


public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
    private readonly IConfiguration _configuration;

    private readonly IModel _channel;
    private readonly IConnection _connection;
    public RabbitMQPublisher(IConfiguration configuration)
    {
        _configuration = configuration;

        string hostName = _configuration["RabbitMQ_HostName"];
        string userName = _configuration["RabbitMQ_UserName"];
        string password = _configuration["RabbitMQ_Password"];
        string port = _configuration["RabbitMQ_Port"];

        ConnectionFactory connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = Convert.ToInt32(port)
        };

        _connection = connectionFactory.CreateConnection();

        _channel = _connection.CreateModel();
       
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }



    public void Publish<T>(Dictionary<string, object> headers, T message)
    {
        string messageJson = JsonSerializer.Serialize(message);
        byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);

        // Exchange Creation

        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;

        _channel.ExchangeDeclare(exchange: exchangeName,
            //type: ExchangeType.Direct,
            //type: ExchangeType.Topic,
            type: ExchangeType.Headers,
            durable: true);

        // Publish Message

        var basicProperties = _channel.CreateBasicProperties();
        basicProperties.Headers = headers;


        _channel.BasicPublish(exchangeName,
            routingKey: string.Empty,
            basicProperties: basicProperties,
            body: messageBodyInBytes);

    }



    
    public void Publish<T>(string routingKey, T message)
    {
        string messageJson = JsonSerializer.Serialize(message);
        byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);

        // Exchange Creation

        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;

        _channel.ExchangeDeclare(exchange: exchangeName,
            type: ExchangeType.Direct,
            //type: ExchangeType.Topic,
            //type: ExchangeType.Headers,
            durable: true);

        // Publish Message
        _channel.BasicPublish(exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: messageBodyInBytes);

    }
    
    

}
