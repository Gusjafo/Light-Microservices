using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Data;
using ProductService.Messaging.Consumers;

namespace ProductService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=products.db";

        services.AddDbContext<ProductContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }

    public static IServiceCollection AddProductMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<OrderCreatedConsumer>();
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            busConfigurator.UsingRabbitMq((context, cfg) =>
            {
                var rabbitSection = configuration.GetSection("RabbitMq");
                var host = rabbitSection.GetValue<string>("Host") ?? "rabbitmq";
                var virtualHost = rabbitSection.GetValue<string>("VirtualHost") ?? "/";
                var username = rabbitSection.GetValue<string>("Username") ?? "guest";
                var password = rabbitSection.GetValue<string>("Password") ?? "guest";

                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
