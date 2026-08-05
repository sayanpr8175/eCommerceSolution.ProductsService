
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductsService.DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services)
    {

        // Add data access layer services into the IOC container.

        return services;
    }
}
