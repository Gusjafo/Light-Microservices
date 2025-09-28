using ProductService.Data;
using ProductService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilogLogging();

builder.Services.AddControllers();
builder.Services.AddProductDatabase(builder.Configuration);
builder.Services.AddProductMessaging(builder.Configuration);

var app = builder.Build();

app.MapControllers();
app.ApplyMigrations<ProductContext>();
app.Run();
