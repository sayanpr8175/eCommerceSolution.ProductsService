

using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductsService.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {

        // Add data access layer services into the IOC container.

        return services;
    }
}