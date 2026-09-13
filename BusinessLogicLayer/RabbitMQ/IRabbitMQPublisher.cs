
namespace eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQPublisher
{
    void Publish<T>(string routingKey, T message);
    void Publish<T>(Dictionary<string, object> headers, T message);
}
