
using DataAccessLayer.Context;
using eCommerce.DataAccessLayer.Repositories;
using eCommerce.DataAccessLayer.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductsService.DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services,
        IConfiguration configuration)
    {

        // Updated connection string for reading credentials from env variable in docker.

        string connectionTemplate = configuration.GetConnectionString("DefaultConnection")!;

        string connString = connectionTemplate.Replace("$MYSQL_HOST", Environment.GetEnvironmentVariable("MYSQL_HOST"))
            .Replace("$MYSQL_PASSWORD", Environment.GetEnvironmentVariable("MYSQL_PASSWORD"))
            .Replace("$MYSQL_PORT", Environment.GetEnvironmentVariable("MYSQL_PORT"))
            .Replace("$MYSQL_USER", Environment.GetEnvironmentVariable("MYSQL_USER"))
            .Replace("$MYSQL_DATABASE", Environment.GetEnvironmentVariable("MYSQL_DATABASE"));

        // Add data access layer services into the IOC container.

        services.AddDbContext<ApplicationDbContext>(
            options => { options.UseMySQL(connString);
        });

        services.AddScoped<IProductsRepository, ProductsRepository>();

        
        return services;
    }
}
