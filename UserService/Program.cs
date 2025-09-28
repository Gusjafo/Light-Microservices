using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

ConfigureLogging(builder);

// Add services
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=users.db";
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlite(connectionString));

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

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<UserContext>();
context.Database.Migrate();

// Configure pipeline
app.MapControllers();
app.Run();

static void ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");

        configuration
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                .WriteTo.File(Path.Combine(logDirectory, "info.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7))
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                .WriteTo.File(Path.Combine(logDirectory, "warnings.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7))
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                .WriteTo.File(Path.Combine(logDirectory, "errors.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7));
    });
}
