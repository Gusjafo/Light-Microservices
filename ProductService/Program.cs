using Microsoft.EntityFrameworkCore;
using ProductService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseSqlite("Data Source=products.db"));

var app = builder.Build();

// Configure HTTP request pipeline
app.MapControllers();
app.Run();
