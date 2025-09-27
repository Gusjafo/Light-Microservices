using System;

namespace UserService.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Primary key
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
