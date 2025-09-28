using UserService.Data;
using UserService.Extensions;

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

builder.Services.AddControllers();
builder.Services.AddUserDatabase(builder.Configuration);
builder.Services.AddUserMessaging(builder.Configuration);

var app = builder.Build();

app.UseCors(allowAngularPolicy);

app.MapControllers();
app.ApplyMigrations<UserContext>();
app.Run();
