using EventHubService.Extensions;
using EventHubService.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilogLogging();

const string allowAngularPolicy = "AllowAngular";

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowAngularPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEventHubServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseCors(allowAngularPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>(NotificationHub.HubPath).RequireAuthorization();

app.Run();
