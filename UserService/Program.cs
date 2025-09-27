using Microsoft.EntityFrameworkCore;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlite("Data Source=users.db"));

var app = builder.Build();

// Configure pipeline
app.MapControllers();
app.Run();
