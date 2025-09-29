using MassTransit;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Data;
using OrderService.Services;

namespace OrderService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderControllers(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        return services;
    }

    public static IServiceCollection AddOrderDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        return services;
    }

    public static IServiceCollection AddOrderApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderCreationService, OrderCreationService>();
        return services;
    }

    public static IServiceCollection AddOrderMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(busConfigurator =>
        {
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
            });
        });

        return services;
    }
}
