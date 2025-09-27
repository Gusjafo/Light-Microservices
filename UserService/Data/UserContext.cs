using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public class UserContext(DbContextOptions<UserContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}
