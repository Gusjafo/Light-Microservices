using System.Threading;
using System.Threading.Tasks;
using IAMService.Models;

namespace IAMService.Services;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
