using OrderService.Data;
using OrderService.Extensions;

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

builder.Services.AddOrderControllers();
builder.Services.AddOrderDatabase(builder.Configuration);
builder.Services.AddServiceEndpoints(builder.Configuration);
builder.Services.AddResilientHttpClients();
builder.Services.AddOrderApplicationServices();
builder.Services.AddOrderMessaging(builder.Configuration);

var app = builder.Build();

app.UseCors(allowAngularPolicy);

app.MapControllers();
app.ApplyMigrations<OrderContext>();
app.Run();
