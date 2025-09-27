using Microsoft.EntityFrameworkCore;
using OrderService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddDbContext<OrderContext>(options =>
    options.UseSqlite("Data Source=orders.db"));

var app = builder.Build();

app.MapControllers();
app.Run();
