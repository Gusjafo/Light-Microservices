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

// Bind endpoints
builder.Services.Configure<ServiceEndpoints>(builder.Configuration.GetSection("Services"));

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
    http.BaseAddress = new Uri(endpoints.User);
    http.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(RetryPolicy())
.AddPolicyHandler(CircuitBreakerPolicy());

builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>((sp, http) =>
{
    var endpoints = sp.GetRequiredService<IOptions<ServiceEndpoints>>().Value;
    http.BaseAddress = new Uri(endpoints.Product);
    http.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(RetryPolicy())
.AddPolicyHandler(CircuitBreakerPolicy());

// App services
builder.Services.AddScoped<IOrderCreationService, OrderCreationService>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<OrderContext>();
context.Database.Migrate();

app.MapControllers();
app.Run();
