using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderService.Data;
using OrderService.External;
using OrderService.Services;
using System.Net;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Controllers & JSON
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// EF Core (SQLite)
builder.Services.AddDbContext<OrderContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Bind endpoints from configuration (validated on startup)
builder.Services
    .AddOptions<ServiceEndpoints>()
    .Bind(builder.Configuration.GetRequiredSection("Services"))
    .ValidateDataAnnotations()
    .Validate(
        endpoints => endpoints.User is { IsAbsoluteUri: true },
        "The user service base URL must be an absolute URI.")
    .Validate(
        endpoints => endpoints.Product is { IsAbsoluteUri: true },
        "The product service base URL must be an absolute URI.")
    .ValidateOnStart();

// Resilience policies
static IAsyncPolicy<HttpResponseMessage> RetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retryAttempt)));

static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(30));

// Typed clients with base addresses from config
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>((sp, http) =>
{
    var endpoints = sp.GetRequiredService<IOptions<ServiceEndpoints>>().Value;
    http.BaseAddress = endpoints.User;
    http.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(RetryPolicy())
.AddPolicyHandler(CircuitBreakerPolicy());

builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>((sp, http) =>
{
    var endpoints = sp.GetRequiredService<IOptions<ServiceEndpoints>>().Value;
    http.BaseAddress = endpoints.Product;
    http.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(RetryPolicy())
.AddPolicyHandler(CircuitBreakerPolicy());

// App services
builder.Services.AddScoped<IOrderCreationService, OrderCreationService>();

builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();

    busConfigurator.UsingRabbitMq((context, cfg) =>
    {
        var rabbitSection = builder.Configuration.GetSection("RabbitMq");
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

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<OrderContext>();
context.Database.Migrate();

app.MapControllers();
app.Run();
