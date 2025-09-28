using UserService.Data;
using UserService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilogLogging();

builder.Services.AddControllers();
builder.Services.AddUserDatabase(builder.Configuration);
builder.Services.AddUserMessaging(builder.Configuration);

var app = builder.Build();

app.MapControllers();
app.ApplyMigrations<UserContext>();
app.Run();
