using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IAMService.Models;
using IAMService.Options;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace IAMService.Services;

public sealed class AuthService : IAuthService
{
    private readonly AuthUsersOptions _userOptions;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthService> _logger;
    private readonly SigningCredentials _signingCredentials;

    public AuthService(AuthUsersOptions userOptions, JwtOptions jwtOptions, ILogger<AuthService> logger)
    {
        _userOptions = userOptions;
        _jwtOptions = jwtOptions;
        _logger = logger;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public Task<LoginResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = _userOptions.Users.FirstOrDefault(u => string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            _logger.LogWarning("Login attempt failed for {Email}: user not found.", request.Email);
            return Task.FromResult<LoginResponse?>(null);
        }

        if (!string.Equals(user.Password, request.Password, StringComparison.Ordinal))
        {
            _logger.LogWarning("Login attempt failed for {Email}: invalid password.", request.Email);
            return Task.FromResult<LoginResponse?>(null);
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var claims = BuildClaims(user);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("User {UserId} authenticated successfully.", user.Id);

        return Task.FromResult<LoginResponse?>(new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = user.Roles,
            Token = tokenValue,
            ExpiresAtUtc = expiresAtUtc
        });
    }

    private static IEnumerable<Claim> BuildClaims(AuthUser user)
    {
        yield return new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString());
        yield return new Claim(JwtRegisteredClaimNames.Email, user.Email);
        yield return new Claim(ClaimTypes.Email, user.Email);

        foreach (var role in user.Roles)
        {
            yield return new Claim("role", role);
        }
    }
}
