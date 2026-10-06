

namespace eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;

public interface IserviceBusPublisher
{
    Task Publish<T>(Dictionary<string, object> headers, T message);
}
