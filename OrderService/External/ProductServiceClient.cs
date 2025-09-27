using System.Net;
using OrderService.Models;

namespace OrderService.External
{
    // Assumes Product Service exposes: GET /api/products/{id} => ProductDto or 404
    public class ProductServiceClient(HttpClient http) : IProductServiceClient
    {
        private readonly HttpClient _http = http;

        public async Task<ProductDto?> GetProductAsync(Guid productId, CancellationToken ct)
        {
            var resp = await _http.GetAsync($"/api/products/{productId}", ct);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: ct);
        }
    }
}