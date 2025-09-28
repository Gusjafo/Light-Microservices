using EventHubService.Messaging.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventHubService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventHubServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR();
        services.AddControllers();

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<OrderCreatedEventConsumer>();
            busConfigurator.AddConsumer<StockDecreasedEventConsumer>();
            busConfigurator.AddConsumer<OrderFailedEventConsumer>();
            busConfigurator.AddConsumer<UserCreatedEventConsumer>();

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
