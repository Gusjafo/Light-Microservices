using OrderService.Data;
using OrderService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilogLogging();

builder.Services.AddOrderControllers();
builder.Services.AddOrderDatabase(builder.Configuration);
builder.Services.AddServiceEndpoints(builder.Configuration);
builder.Services.AddResilientHttpClients();
builder.Services.AddOrderApplicationServices();
builder.Services.AddOrderMessaging(builder.Configuration);

var app = builder.Build();

app.MapControllers();
app.ApplyMigrations<OrderContext>();
app.Run();
