using MassTransit;
using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderService.Data;
using OrderService.External;
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

    public static IServiceCollection AddServiceEndpoints(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ServiceEndpoints>()
            .Bind(configuration.GetRequiredSection("Services"))
            .ValidateDataAnnotations()
            .Validate(
                endpoints => endpoints.User is { IsAbsoluteUri: true },
                "The user service base URL must be an absolute URI.")
            .Validate(
                endpoints => endpoints.Product is { IsAbsoluteUri: true },
                "The product service base URL must be an absolute URI.")
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddResilientHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IUserServiceClient, UserServiceClient>((sp, client) =>
            {
                var endpoints = sp.GetRequiredService<IOptions<ServiceEndpoints>>().Value;
                client.BaseAddress = endpoints.User;
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .AddPolicyHandler(HttpClientPolicies.CreateRetryPolicy())
            .AddPolicyHandler(HttpClientPolicies.CreateCircuitBreakerPolicy());

        services.AddHttpClient<IProductServiceClient, ProductServiceClient>((sp, client) =>
            {
                var endpoints = sp.GetRequiredService<IOptions<ServiceEndpoints>>().Value;
                client.BaseAddress = endpoints.Product;
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .AddPolicyHandler(HttpClientPolicies.CreateRetryPolicy())
            .AddPolicyHandler(HttpClientPolicies.CreateCircuitBreakerPolicy());

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
