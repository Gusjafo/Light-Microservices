using System.Threading;
using System.Threading.Tasks;
using IAMService.Models;
using IAMService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAMService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.AuthenticateAsync(request, cancellationToken);
        if (response is null)
        {
            return Unauthorized(new { error = "Credenciales inválidas" });
        }

        return Ok(response);
    }
}
