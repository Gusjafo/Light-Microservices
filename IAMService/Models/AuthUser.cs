using System;

namespace IAMService.Models;

public sealed class AuthUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}
