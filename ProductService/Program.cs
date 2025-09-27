using Microsoft.EntityFrameworkCore;
using ProductService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=products.db";
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Configure HTTP request pipeline
app.MapControllers();
app.Run();
