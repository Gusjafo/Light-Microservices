using ProductService.Data;
using ProductService.Extensions;

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
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddProductDatabase(builder.Configuration);
builder.Services.AddProductMessaging(builder.Configuration);

var app = builder.Build();

app.UseCors(allowAngularPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.ApplyMigrations<ProductContext>();
app.Run();
