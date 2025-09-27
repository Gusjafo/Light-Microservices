using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=users.db";
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<UserContext>();
context.Database.Migrate();

// Configure pipeline
app.MapControllers();
app.Run();
