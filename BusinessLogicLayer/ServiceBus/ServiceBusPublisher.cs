
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;

public class ServiceBusPublisher : IserviceBusPublisher
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IConfiguration _configuration;
    private readonly ServiceBusSender _sender;
    public ServiceBusPublisher(ServiceBusClient serviceBusClient, IConfiguration configuration)
    {
        _serviceBusClient = serviceBusClient;
        _configuration = configuration;

        _sender = _serviceBusClient.CreateSender(_configuration["ServiceBus:ServiceBus_ProductsTopic"]);
    }
    public async Task Publish<T>(Dictionary<string, object> headers, T message)
    {
        string messageJson = JsonSerializer.Serialize(message);
        ServiceBusMessage servicebusMsg = new ServiceBusMessage(messageJson);

        foreach (var header in headers)
        {
            servicebusMsg.ApplicationProperties[header.Key] = header.Value;
        }

        await _sender.SendMessageAsync(servicebusMsg);

    }
}
