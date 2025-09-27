using System.Net;

namespace OrderService.External
{
    // Assumes User Service exposes: GET /api/users/{id} => 200 if exists, 404 if not
    public class UserServiceClient(HttpClient http) : IUserServiceClient
    {
        private readonly HttpClient _http = http;

        public async Task<bool> UserExistsAsync(Guid userId, CancellationToken ct)
        {
            var resp = await _http.GetAsync($"/api/users/{userId}", ct);
            if (resp.StatusCode == HttpStatusCode.NotFound) return false;
            resp.EnsureSuccessStatusCode(); // throws for 5xx/4xx (not 404)
            return true;
        }
    }
}