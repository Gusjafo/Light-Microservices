using System;

namespace IAMService.Models;

public sealed class LoginResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string[] Roles { get; init; } = Array.Empty<string>();
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
