using Microsoft.EntityFrameworkCore;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=users.db";
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Configure pipeline
app.MapControllers();
app.Run();
