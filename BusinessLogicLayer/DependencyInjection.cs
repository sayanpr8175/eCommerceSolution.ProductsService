
using eCommerce.BusinessLogicLayer.Mappers;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Validators;
using eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductsService.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {

        // Add data access layer services into the IOC container.

        services.AddAutoMapper(typeof(ProductAddRequestToProductMappingProfile).Assembly);

        services.AddScoped<IProductsService, eCommerce.BusinessLogicLayer.Services.ProductsService>();

        services.AddTransient<IRabbitMQPublisher, RabbitMQPublisher>();

        services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();

        return services;
    }
}